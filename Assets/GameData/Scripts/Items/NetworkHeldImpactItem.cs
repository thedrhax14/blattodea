using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Utility.Template;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Core;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Playables;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// <para>Networked held-item action that plays a CAS <see cref="AnimationAsset"/>, then applies server-authoritative
/// force to the first rigidbody hit by a raycast.</para>
/// <para>Place this component on the held item GameObject under the spawned player hierarchy so it can find the
/// parent <see cref="CharacterAnimationComponent"/> and use the item transform or assigned ray origin.</para>
/// <para>Configure <see cref="_useAnimation"/>, <see cref="_useAction"/>, and optionally <see cref="_rayOrigin"/>.</para>
/// <para>Keep this object under a FishNet-owned player object so only the owner subscribes to input and sends use requests.</para>
/// <para>The hit target should be server-driven and replicated with FishNet networking components for the force result to stay visible on clients.</para>
/// </summary>
[AddComponentMenu(CasNames.Path_ComponentMenu + "Examples/Network Held Impact Item")]
[DisallowMultipleComponent]
public sealed class NetworkHeldImpactItem : TickNetworkBehaviour
{
    private const uint UnsetTick = uint.MaxValue;
    private static readonly Color MissDebugRayColor = Color.red;
    private static readonly Color HitDebugRayColor = Color.green;

    [Header("Animation")]
    [SerializeField]
    private AnimationAsset _useAnimation;
    [SerializeField]
    private CharacterAnimationComponent _characterAnimation;

    [Header("Input")]
    [SerializeField]
    private InputActionReference _useAction;

    [Header("Audio")]
    [SerializeField]
    private AudioSource _impactAudioSource;
    [SerializeField]
    private AudioClip _impactAudioClip;
    [SerializeField]
    [Range(0f, 1f)]
    private float _impactAudioVolume = 1f;

    [Header("Raycast")]
    [SerializeField]
    private Transform _rayOrigin;
    [SerializeField]
    private Transform _ignoredRoot;
    [SerializeField]
    [Min(0f)]
    private float _rayDistance = 2f;
    [SerializeField]
    private LayerMask _hitMask = Physics.DefaultRaycastLayers;
    [SerializeField]
    private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Impact")]
    [SerializeField]
    [Min(0f)]
    private float _impactDelaySeconds = 0.15f;
    [SerializeField]
    [Min(0f)]
    private float _cooldownSeconds = 0.5f;
    [SerializeField]
    private float _force = 10f;
    [SerializeField]
    private ForceMode _forceMode = ForceMode.Impulse;

    [Header("Sync")]
    [SerializeField]
    [Min(0)]
    private uint _animationLeadTicks = 1;

    private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

    private uint _nextUseTick;
    private uint _scheduledAnimationTick = UnsetTick;
    private uint _scheduledImpactTick = UnsetTick;
    private Vector3 _scheduledImpactOrigin;
    private Vector3 _scheduledImpactDirection;
    private bool _isSubscribedToInput;
    private bool _ownsEnabledInputAction;

    private void Awake()
    {
        SetTickCallbacks(TickCallback.Tick);
        ResolveReferences();
    }

    private void OnEnable()
    {
        RefreshInputSubscription();
    }

    private void OnDisable()
    {
        UnsubscribeFromInput();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} started on client. IsOwner={IsOwner}", this);
        RefreshInputSubscription();
    }

    public override void OnStopClient()
    {
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} stopped on client. Removing input subscription if present.", this);
        UnsubscribeFromInput();
        base.OnStopClient();
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} ownership updated on client. PreviousOwner={(prevOwner == null ? "null" : prevOwner.ClientId.ToString())}, IsOwner={IsOwner}", this);
        RefreshInputSubscription();
    }

    /// <summary>
    /// Requests item use for the owning player.
    /// Call this from the assigned <see cref="InputActionReference"/> or from an external equip/input controller.
    /// </summary>
    public void TryUseItem()
    {
        ResolveReferences();
        ValidateRuntimeReferences();

        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} received a use request. ServerStarted={IsServerStarted}, ClientStarted={IsClientStarted}, IsOwner={IsOwner}", this);

        if (!IsServerStarted && !IsClientStarted)
        {
            Debug.LogWarning($"{nameof(NetworkHeldImpactItem)} on {name} ignored use request because networking is not started.", this);
            return;
        }

        Vector3 origin = GetRayOrigin();
        Vector3 direction = GetRayDirection();

        if (IsServerStarted)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} is handling the use request on the server.", this);
            HandleUseRequest(origin, direction);
            return;
        }

        if (!IsOwner)
        {
            Debug.LogWarning($"{nameof(NetworkHeldImpactItem)} on {name} ignored use request because this client is not the owner.", this);
            return;
        }

        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} is sending a use request to the server.", this);
        ServerUseItemRpc(origin, direction);
    }

    protected override void TimeManager_OnTick()
    {
        uint currentTick = TimeManager.LocalTick;

        if (_scheduledAnimationTick != UnsetTick && currentTick >= _scheduledAnimationTick)
        {
            uint animationTick = _scheduledAnimationTick;
            _scheduledAnimationTick = UnsetTick;
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} reached scheduled animation tick {animationTick}. LocalTick={currentTick}", this);
            PlayUseAnimation(GetAnimationStartOffset(animationTick));
        }

        if (!IsServerStarted || _scheduledImpactTick == UnsetTick || currentTick < _scheduledImpactTick)
        {
            return;
        }

        Vector3 origin = _scheduledImpactOrigin;
        Vector3 direction = _scheduledImpactDirection;

        _scheduledImpactTick = UnsetTick;
        _scheduledImpactOrigin = default;
        _scheduledImpactDirection = default;

        PerformImpact(origin, direction);
    }

    [ServerRpc]
    private void ServerUseItemRpc(Vector3 origin, Vector3 direction)
    {
        HandleUseRequest(origin, direction);
    }

    [ObserversRpc]
    private void ObserversScheduleUseAnimationRpc(uint startTick)
    {
        uint estimatedServerTick = GetEstimatedServerTick();
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} received animation schedule RPC for server tick {startTick}. LocalTick={TimeManager.LocalTick}, EstimatedServerTick={estimatedServerTick}", this);
        ScheduleOrPlayAnimation(startTick);
    }

    [ObserversRpc]
    private void ObserversPlayImpactAudioRpc()
    {
        PlayImpactAudio();
    }

    [Server]
    private void HandleUseRequest(Vector3 origin, Vector3 direction)
    {
        uint currentTick = TimeManager.LocalTick;
        if (currentTick < _nextUseTick)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} ignored use request because it is on cooldown until tick {_nextUseTick}. CurrentTick={currentTick}", this);
            return;
        }

        Vector3 normalizedDirection = direction.sqrMagnitude > Mathf.Epsilon
            ? direction.normalized
            : GetFallbackDirection();

        _nextUseTick = currentTick + SecondsToTicks(_cooldownSeconds);

        uint startTick = currentTick + _animationLeadTicks;
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} scheduled animation for tick {startTick}. CurrentTick={currentTick}, LeadTicks={_animationLeadTicks}", this);
        ScheduleOrPlayAnimation(startTick);
        ObserversScheduleUseAnimationRpc(startTick);

        uint impactTick = startTick + SecondsToTicks(_impactDelaySeconds);
        ScheduleOrPerformImpact(impactTick, origin, normalizedDirection);
    }

    private void ScheduleOrPlayAnimation(uint startTick)
    {
        if (TimeManager == null)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} is playing the animation immediately for scheduled tick {startTick}. LocalTick={(TimeManager == null ? UnsetTick : TimeManager.LocalTick)}", this);
            PlayUseAnimation(GetAnimationStartOffset(startTick));
            return;
        }

        uint localStartTick = startTick;
        if (!IsServerStarted)
        {
            uint estimatedServerTick = GetEstimatedServerTick();
            if (estimatedServerTick >= startTick)
            {
                float catchUpOffset = (estimatedServerTick - startTick) * (float)TimeManager.TickDelta;
                Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} is playing the animation immediately for past server tick {startTick}. LocalTick={TimeManager.LocalTick}, EstimatedServerTick={estimatedServerTick}, CatchUpOffset={catchUpOffset}", this);
                PlayUseAnimation(catchUpOffset);
                return;
            }

            localStartTick = TimeManager.LocalTick + (startTick - estimatedServerTick);
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} mapped server tick {startTick} to local tick {localStartTick}. LocalTick={TimeManager.LocalTick}, EstimatedServerTick={estimatedServerTick}", this);
        }

        if (TimeManager.LocalTick >= localStartTick)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} is playing the animation immediately for mapped local tick {localStartTick}. LocalTick={TimeManager.LocalTick}", this);
            PlayUseAnimation(GetAnimationStartOffset(localStartTick));
            return;
        }

        _scheduledAnimationTick = localStartTick;
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} queued animation playback for local tick {localStartTick}. CurrentLocalTick={TimeManager.LocalTick}", this);
    }

    private uint GetEstimatedServerTick()
    {
        if (TimeManager == null)
        {
            return UnsetTick;
        }

        if (IsServerStarted)
        {
            return TimeManager.LocalTick;
        }

        uint estimatedServerTick = TimeManager.LastPacketTick.Value();
        if (estimatedServerTick == UnsetTick)
        {
            return TimeManager.Tick;
        }

        return estimatedServerTick;
    }

    [Server]
    private void ScheduleOrPerformImpact(uint impactTick, Vector3 origin, Vector3 direction)
    {
        if (TimeManager.LocalTick >= impactTick)
        {
            PerformImpact(origin, direction);
            return;
        }

        _scheduledImpactTick = impactTick;
        _scheduledImpactOrigin = origin;
        _scheduledImpactDirection = direction;
    }

    private void PlayUseAnimation(float startTime = 0f)
    {
        ResolveReferences();
        ValidateAnimationReferences();

        float playLength = _useAnimation.GetPlayLength();
        float clampedStartTime = playLength > 0f ? Mathf.Min(startTime, playLength) : 0f;
        bool played = _characterAnimation.PlayAnimation(_useAnimation, clampedStartTime);
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} requested animation playback. StartTime={clampedStartTime}, PlayLength={playLength}, Played={played}, CharacterAnimation={_characterAnimation.name}, AnimationAsset={_useAnimation.name}", this);
    }

    [Server]
    private void PerformImpact(Vector3 origin, Vector3 direction)
    {
        ObserversPlayImpactAudioRpc();

        if (!TryGetHit(origin, direction, out RaycastHit hit))
        {
            DrawDebugRay(origin, direction, _rayDistance, MissDebugRayColor);
            return;
        }

        DrawDebugRay(origin, direction, hit.distance, HitDebugRayColor);

        Rigidbody targetRigidbody = hit.rigidbody;
        if (targetRigidbody == null || targetRigidbody.isKinematic)
        {
            return;
        }

        Vector3 forceVector = direction.normalized * _force;
        targetRigidbody.AddForceAtPosition(forceVector, hit.point, _forceMode);

        if (targetRigidbody.GetComponentInParent<NetworkObject>() == null)
        {
            Debug.LogWarning($"{nameof(NetworkHeldImpactItem)} hit {targetRigidbody.name}, but it has no {nameof(NetworkObject)}. Force will only exist on the server.", targetRigidbody);
        }
    }

    private bool TryGetHit(Vector3 origin, Vector3 direction, out RaycastHit closestHit)
    {
        closestHit = default;

        Vector3 normalizedDirection = direction.sqrMagnitude > Mathf.Epsilon
            ? direction.normalized
            : GetFallbackDirection();

        int hitCount = Physics.RaycastNonAlloc(origin, normalizedDirection, _hitBuffer, _rayDistance, _hitMask,
            _queryTriggerInteraction);
        if (hitCount <= 0)
        {
            return false;
        }

        Transform ignoredRoot = _ignoredRoot != null ? _ignoredRoot : transform.root;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _hitBuffer[i];
            if (hit.collider == null)
            {
                continue;
            }

            if (ignoredRoot != null && hit.transform.IsChildOf(ignoredRoot))
            {
                continue;
            }

            if (hit.distance >= closestDistance)
            {
                continue;
            }

            closestDistance = hit.distance;
            closestHit = hit;
        }

        return closestDistance < float.PositiveInfinity;
    }

    private float GetAnimationStartOffset(uint startTick)
    {
        if (TimeManager == null || startTick == UnsetTick)
        {
            return 0f;
        }

        uint currentTick = TimeManager.LocalTick;
        if (currentTick <= startTick)
        {
            return 0f;
        }

        return (currentTick - startTick) * (float) TimeManager.TickDelta;
    }

    private uint SecondsToTicks(float value)
    {
        if (TimeManager == null || value <= 0f)
        {
            return 0u;
        }

        return TimeManager.TimeToTicks(value, TickRounding.RoundUp);
    }

    private Vector3 GetRayOrigin()
    {
        Transform origin = _rayOrigin != null ? _rayOrigin : transform;
        return origin.position;
    }

    private Vector3 GetRayDirection()
    {
        Transform origin = _rayOrigin != null ? _rayOrigin : transform;
        return origin.forward;
    }

    private Vector3 GetFallbackDirection()
    {
        return transform.forward.sqrMagnitude > Mathf.Epsilon ? transform.forward.normalized : Vector3.forward;
    }

    private void DrawDebugRay(Vector3 origin, Vector3 direction, float distance, Color color)
    {
        Vector3 normalizedDirection = direction.sqrMagnitude > Mathf.Epsilon
            ? direction.normalized
            : GetFallbackDirection();
        Debug.DrawRay(origin, normalizedDirection * distance, color, 1f);
    }

    private void PlayImpactAudio()
    {
        ResolveReferences();

        if (_impactAudioSource == null || _impactAudioClip == null)
        {
            return;
        }

        _impactAudioSource.pitch = 1f;
        _impactAudioSource.PlayOneShot(_impactAudioClip, _impactAudioVolume);
    }

    private void OnUseActionPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} received input action '{context.action.name}' with phase {context.phase}.", this);
        TryUseItem();
    }

    private void RefreshInputSubscription()
    {
        if (!isActiveAndEnabled)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} skipped input subscription because the component is disabled.", this);
            UnsubscribeFromInput();
            return;
        }

        if (!IsClientStarted || !IsOwner)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} skipped input subscription. ClientStarted={IsClientStarted}, IsOwner={IsOwner}", this);
            UnsubscribeFromInput();
            return;
        }

        ValidateInputReference();

        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} preparing input subscription for action '{_useAction.action.name}'. ActionEnabled={_useAction.action.enabled}", this);

        if (_isSubscribedToInput)
        {
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} already has its input subscription active.", this);
            return;
        }

        if (!_useAction.action.enabled)
        {
            _useAction.action.Enable();
            _ownsEnabledInputAction = true;
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} enabled input action '{_useAction.action.name}'.", this);
        }

        _useAction.action.performed += OnUseActionPerformed;
        _isSubscribedToInput = true;
        Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} subscribed to input action '{_useAction.action.name}'.", this);
    }

    private void UnsubscribeFromInput()
    {
        if (_useAction != null && _useAction.action != null && _isSubscribedToInput)
        {
            _useAction.action.performed -= OnUseActionPerformed;
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} unsubscribed from input action '{_useAction.action.name}'.", this);
        }

        if (_useAction != null && _useAction.action != null && _ownsEnabledInputAction)
        {
            _useAction.action.Disable();
            Debug.Log($"{nameof(NetworkHeldImpactItem)} on {name} disabled input action '{_useAction.action.name}'.", this);
        }

        _isSubscribedToInput = false;
        _ownsEnabledInputAction = false;
    }

    private void ResolveReferences()
    {
        if (_characterAnimation == null)
        {
            _characterAnimation = GetComponentInParent<CharacterAnimationComponent>();
        }

        if (_rayOrigin == null)
        {
            _rayOrigin = transform;
        }

        if (_ignoredRoot == null)
        {
            _ignoredRoot = transform.root;
        }

        if (_impactAudioSource == null)
        {
            _impactAudioSource = GetComponent<AudioSource>();
        }
    }

    private void ValidateRuntimeReferences()
    {
        ValidateAnimationReferences();
        ValidateInputReference();
    }

    private void ValidateAnimationReferences()
    {
        if (_useAnimation == null)
        {
            throw new InvalidOperationException($"{nameof(NetworkHeldImpactItem)} on {name} requires {nameof(_useAnimation)} to be assigned.");
        }

        if (_characterAnimation == null)
        {
            throw new MissingReferenceException($"{nameof(NetworkHeldImpactItem)} on {name} could not resolve a {nameof(CharacterAnimationComponent)} in its parent hierarchy.");
        }
    }

    private void ValidateInputReference()
    {
        if (_useAction == null)
        {
            throw new InvalidOperationException($"{nameof(NetworkHeldImpactItem)} on {name} requires {nameof(_useAction)} to be assigned.");
        }

        if (_useAction.action == null)
        {
            throw new MissingReferenceException($"{nameof(NetworkHeldImpactItem)} on {name} has an {nameof(_useAction)} reference without a valid InputAction.");
        }
    }
}