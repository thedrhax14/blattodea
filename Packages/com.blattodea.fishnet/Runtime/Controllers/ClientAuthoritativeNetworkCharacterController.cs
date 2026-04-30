using System;
using Blattodea.Core.Models;
using Blattodea.FishNet.Interactions;
using FishNet.Connection;
using FishNet.Object;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Camera;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Core;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Modifiers.IK;
using UnityEngine;

#if UNITY_INPUT_SYSTEM_ENABLED
using UnityEngine.InputSystem;
#endif

namespace Blattodea.FishNet.Controllers
{
    public enum NetworkCharacterMovementState
    {
        Default,
        Idle,
        CrouchWalk,
        Walk,
        Jog,
        Sprint
    }

    [Serializable]
    public struct NetworkMovementGaitSettings
    {
        [Min(0f)] public float velocity;
        [Min(0f)] public float acceleration;
        [Min(0f)] public float deceleration;
        [Min(0f)] public float rotationSpeed;
    }

    public struct NetworkSurfaceTraceInfo
    {
        public bool hasHit;
        public RaycastHit hitInfo;
        public bool canSnapToGround;
    }

    public struct NetworkCharacterReplicatedState
    {
        public Vector3 LookInput;
        public Vector2 MoveInput;
        public float Gait;
        public bool IsGrounded;
        public bool IsCrouching;
        public bool IsInAir;
        public NetworkObject ActiveInteractionObject;
    }

    [RequireComponent(typeof(CharacterController))]
    public class ClientAuthoritativeNetworkCharacterController : NetworkBehaviour
    {
        protected static readonly int Animator_MoveX = Animator.StringToHash("MoveX");
        protected static readonly int Animator_MoveY = Animator.StringToHash("MoveY");
        protected static readonly int Animator_Gait = Animator.StringToHash("Gait");
        protected static readonly int Animator_ViewWeight = Animator.StringToHash("ViewWeight");
        protected static readonly int Animator_CrouchWeight = Animator.StringToHash("CrouchWeight");
        protected static readonly int Animator_Crouch = Animator.StringToHash("Crouch");
        protected static readonly int Animator_IsInAir = Animator.StringToHash("IsInAir");
        protected static readonly int Animator_IsFirstPerson = Animator.StringToHash("IsFirstPerson");
        protected static readonly int Animator_IsMoving = Animator.StringToHash("IsMoving");

        public float Gait => _gait;
        public Vector3 LookInput => _lookInput;
        public Vector2 MoveInput => _moveInput;
        public bool IsGrounded => !_isInAir;
        public bool IsCrouching => _isCrouching;
        public bool IsInAir => _isInAir;
        public Transform RightHandIkTarget => _interaction?.GetRightHandTarget();
        public Transform LeftHandIkTarget => _interaction?.GetLeftHandTarget();

        [Header("Network")]
        [Tooltip("How quickly remote proxies interpolate toward the latest authoritative state.")]
        [SerializeField, Min(0f)] protected float remoteInterpolationSpeed = 12f;
        [Tooltip("Minimum time between owner state sends.")]
        [SerializeField, Min(0.01f)] protected float sendInterval = 0.05f;

        [Header("Inputs")]
        [Tooltip("Mouse and stick look sensitivity multiplier.")]
        [SerializeField, Min(0f)] protected float lookSensitivity = 1f;

    #if UNITY_INPUT_SYSTEM_ENABLED
        [Tooltip("Movement input action reference.")]
        [SerializeField] protected InputActionReference moveInputAction;
        [Tooltip("Look input action reference.")]
        [SerializeField] protected InputActionReference lookInputAction;
        [Tooltip("Sprint input action reference.")]
        [SerializeField] protected InputActionReference sprintInputAction;
        [Tooltip("Jump input action reference.")]
        [SerializeField] protected InputActionReference jumpInputAction;
        [Tooltip("Interact input action reference.")]
        [SerializeField] protected InputActionReference interactInputAction;
        [Tooltip("Crouch input action reference.")]
        [SerializeField] protected InputActionReference crouchInputAction;
        [Tooltip("Walk toggle input action reference.")]
        [SerializeField] protected InputActionReference toggleWalkInputAction;
    #endif

        [Header("Movement")]
        [Tooltip("Collision layers used for ground probing.")]
        [SerializeField] protected LayerMask groundLayer = 1;
        [Tooltip("Minimum horizontal speed considered movement.")]
        [SerializeField, Min(0f)] protected float movingVelocityThreshold = 0.1f;
        [Tooltip("Input magnitude threshold that separates walk from jog.")]
        [SerializeField, Range(0f, 1f)] protected float walkInputThreshold = 0.5f;
        [Tooltip("If enabled the controller stays in walk instead of switching to jog.")]
        [SerializeField] protected bool useDefaultWalk;
        [Tooltip("Deceleration used when fully idling.")]
        [SerializeField, Min(0f)] protected float idleDeceleration = 50f;
        [Tooltip("Interpolation speed used for the gait parameter.")]
        [SerializeField, Min(0f)] protected float gaitSmoothing = 12f;
        [SerializeField] protected NetworkMovementGaitSettings walkGait = new NetworkMovementGaitSettings
        {
            velocity = 2f,
            acceleration = 2.2f,
            deceleration = 3f,
            rotationSpeed = 7f
        };
        [SerializeField] protected NetworkMovementGaitSettings jogGait = new NetworkMovementGaitSettings
        {
            velocity = 3f,
            acceleration = 2.2f,
            deceleration = 3f,
            rotationSpeed = 7f
        };
        [SerializeField] protected NetworkMovementGaitSettings sprintGait = new NetworkMovementGaitSettings
        {
            velocity = 4.5f,
            acceleration = 2.2f,
            deceleration = 2f,
            rotationSpeed = 7f
        };
        [SerializeField] protected NetworkMovementGaitSettings crouchWalkGait = new NetworkMovementGaitSettings
        {
            velocity = 1.3f,
            acceleration = 2.2f,
            deceleration = 50f,
            rotationSpeed = 7f
        };

        [Header("In Air")]
        [Tooltip("Requested jump height when grounded or within coyote time.")]
        [SerializeField, Min(0f)] protected float jumpHeight = 1f;
        [Tooltip("Delay before entering in-air state when losing ground contact.")]
        [SerializeField, Min(0f)] protected float fallDelay = 0.3f;
        [Tooltip("Gravity magnitude applied while airborne.")]
        [SerializeField, Min(0f)] protected float gravity = 9.81f;
        [Tooltip("Maximum downward velocity while airborne.")]
        [SerializeField, Min(0f)] protected float maxFallVelocity = 25f;
        [Tooltip("Maximum air movement speed.")]
        [SerializeField, Min(0f)] protected float airMaxSpeed = 3f;
        [Tooltip("Acceleration applied to horizontal movement while airborne.")]
        [SerializeField, Min(0f)] protected float airAcceleration = 4f;
        [Tooltip("How much landing momentum is preserved after touching the ground.")]
        [SerializeField, Range(0f, 1f)] protected float landingMomentumFactor = 0.6f;
        [Tooltip("Downward force kept while grounded to maintain contact.")]
        [SerializeField, Min(0f)] protected float groundedStickVelocity = 2f;

        [Header("Grounding Stability")]
        [Tooltip("How long a jump may still trigger after losing ground contact.")]
        [SerializeField, Min(0f)] protected float jumpCoyoteTime = 0.12f;
        [Tooltip("How far below the controller to probe for ground snapping.")]
        [SerializeField, Min(0f)] protected float groundSnapDistance = 0.25f;
        [Tooltip("Maximum walkable slope angle.")]
        [SerializeField, Range(0f, 89f)] protected float maxStableSlopeAngle = 60f;
        [Tooltip("Radius factor used when probing beneath the character.")]
        [SerializeField, Range(0.05f, 1f)] protected float groundProbeRadiusFactor = 0.9f;
        [Tooltip("Minimum falling speed required before entering the in-air state.")]
        [SerializeField, Min(0f)] protected float minFallSpeedToInAir = 1.25f;

        [Header("Rotation")]
        [Tooltip("If enabled the body rotates toward movement direction when moving.")]
        [SerializeField] protected bool orientRotationToMovement = true;
        [Tooltip("If enabled the controller uses first-person style yaw rotation at all times.")]
        [SerializeField] protected bool isFirstPerson;
        [Tooltip("Rotation speed when rotating toward look direction.")]
        [SerializeField, Min(0f)] protected float lookRotationSpeed = 10f;
        [Tooltip("Optional camera driven by this controller. If empty, the first child CharacterCamera is used.")]
        [SerializeField] protected CharacterCamera characterCamera;
        [Tooltip("Optional procedural data owner updated by this controller. If empty, the first available component is used.")]
        [SerializeField] protected CASProceduralDataBehaviour proceduralDataBehaviour;

        [Header("Animation")]
        [Tooltip("Optional animator driven by this controller. If empty, the first child animator is used.")]
        [SerializeField] protected Animator animator;
        [Tooltip("Optional procedural animation component driven by this controller. If empty, the first child component is used.")]
        [SerializeField] protected ProceduralAnimationComponent proceduralAnimation;
        [Tooltip("Interpolation speed used for the animator gait parameter.")]
        [SerializeField, Min(0f)] protected float animatorGaitSmoothing = 12f;
        [Tooltip("Interpolation speed used for the animator move input parameters.")]
        [SerializeField, Min(0f)] protected float animatorMoveSmoothing = 7f;
        [Tooltip("Interpolation speed used for the animator crouch weight parameter.")]
        [SerializeField, Min(0f)] protected float animatorCrouchSmoothing = 12f;

        [Header("Steps")]
        [Tooltip("Step modifier applied when crouching in place while grounded.")]
        [SerializeField] protected StepModifierSettings stepCrouch;
        [Tooltip("Step modifier applied when uncrouching in place while grounded.")]
        [SerializeField] protected StepModifierSettings stepUncrouch;
        [Tooltip("Optional in-place step modifier reserved for item or stance transitions.")]
        [SerializeField] protected StepModifierSettings stepInPlace;
        [Tooltip("Step modifier applied when starting to move while grounded.")]
        [SerializeField] protected StepModifierSettings startMoving;
        [Tooltip("Step modifier applied when stopping while grounded.")]
        [SerializeField] protected StepModifierSettings stopMoving;

        protected CharacterController _characterController;
        protected bool _isInAir;
        protected bool _isGrounded;
        protected bool _wantsToJump;
        protected float _timeSinceLastGrounded;
        protected NetworkSurfaceTraceInfo _surfaceTrace;

        protected bool _isSprintPressed;
        protected bool _isCrouching;
        protected Vector2 _deltaLookInput;
        protected Vector3 _lookInput;
        protected Quaternion _aimRotation = Quaternion.identity;
        protected Quaternion _targetRotation = Quaternion.identity;
        protected Vector2 _moveInput;
        protected Vector3 _velocity;
        protected Vector3 _desiredPlanarVelocity;
        protected bool _wasMoving;
        protected NetworkCharacterMovementState _movementState = NetworkCharacterMovementState.Idle;
        protected NetworkCharacterMovementState _prevMovementState = NetworkCharacterMovementState.Walk;
        protected NetworkMovementGaitSettings _activeGait;
        protected float _gait;
        protected float _animatorGait;
        protected float _animatorCrouchWeight;
        protected float _networkGait;
        protected float _originalHeight;
        protected Vector3 _originalCenter;
        protected ICharacterInteraction _interaction;
        protected Collider _interactionCollider;
        protected Vector3 _lastAnimatorSamplePosition;
        protected bool _hasAnimatorSamplePosition;
        protected float _lastSendTime;
        protected NetworkCharacterReplicatedState _remoteState;
        protected bool _hasRemoteState;

    #if UNITY_INPUT_SYSTEM_ENABLED
        protected bool _inputSubscriptionsActive;
    #endif

        protected virtual void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _originalCenter = _characterController.center;
            _originalHeight = _characterController.height;
            _aimRotation = transform.rotation;
            _activeGait = walkGait;
            if (characterCamera == null)
            {
                characterCamera = GetComponentInChildren<CharacterCamera>(true);
            }
            if (proceduralDataBehaviour == null)
            {
                proceduralDataBehaviour = GetComponent<CASProceduralDataBehaviour>();
                if (proceduralDataBehaviour == null)
                {
                    proceduralDataBehaviour = GetComponentInChildren<CASProceduralDataBehaviour>(true);
                }
            }
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
            if (proceduralAnimation == null)
            {
                proceduralAnimation = GetComponent<ProceduralAnimationComponent>();
                if (proceduralAnimation == null)
                {
                    proceduralAnimation = GetComponentInChildren<ProceduralAnimationComponent>(true);
                }
            }

            _lastAnimatorSamplePosition = transform.position;
            _hasAnimatorSamplePosition = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected virtual void OnEnable()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            RefreshInputSubscriptions();
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            DisableInputSubscriptions();
#endif
            ClearInputs();
        }

        protected virtual void OnDestroy()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            DisableInputSubscriptions();
#endif
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
#if UNITY_INPUT_SYSTEM_ENABLED
            RefreshInputSubscriptions();
#endif
        }

        public override void OnStopClient()
        {
#if UNITY_INPUT_SYSTEM_ENABLED
            DisableInputSubscriptions();
#endif
            ClearInputs();
            base.OnStopClient();
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
#if UNITY_INPUT_SYSTEM_ENABLED
            RefreshInputSubscriptions();
#endif

            if (!IsOwner)
            {
                ClearInputs();
            }
        }

        protected virtual void Update()
        {
            if (IsOwner)
            {
                TickOwner();
                return;
            }

            if (base.IsClientInitialized)
            {
                TickRemoteProxy();
            }
        }

        protected virtual void TickOwner()
        {
            UpdateMovementState();
            UpdateGait();
            UpdateRotation();
            UpdateMovement();

            bool isMoving = HasMoveInputs();
            if (_wasMoving != isMoving)
            {
                OnMovementChange(isMoving);
            }

            _wasMoving = isMoving;
            UpdateAnimatorParameters();
            SyncProceduralData();
            TrySendAuthoritativeState();
            _deltaLookInput = Vector2.zero;
        }

        protected virtual void TickRemoteProxy()
        {
            if (!_hasRemoteState)
            {
                return;
            }

            float alpha = remoteInterpolationSpeed <= 0f
                ? 1f
                : 1f - Mathf.Exp(-remoteInterpolationSpeed * Time.deltaTime);
            _lookInput = Vector3.Lerp(_lookInput, _remoteState.LookInput, alpha);
            _moveInput = Vector2.Lerp(_moveInput, _remoteState.MoveInput, alpha);
            _networkGait = Mathf.Lerp(_networkGait, _remoteState.Gait, alpha);
            _gait = _networkGait;
            _isGrounded = _remoteState.IsGrounded;
            _isInAir = _remoteState.IsInAir;
            _isCrouching = _remoteState.IsCrouching;
            _interaction = ResolveInteraction(_remoteState.ActiveInteractionObject);
            UpdateAnimatorParameters();
            SyncProceduralData();
        }

        protected virtual void TrySendAuthoritativeState()
        {
            if (Time.time < _lastSendTime + sendInterval)
            {
                return;
            }

            _lastSendTime = Time.time;
            NetworkCharacterReplicatedState state = new()
            {
                LookInput = _lookInput,
                MoveInput = _moveInput,
                Gait = _gait,
                IsGrounded = _isGrounded,
                IsCrouching = _isCrouching,
                IsInAir = _isInAir,
                ActiveInteractionObject = GetActiveInteractionNetworkObject()
            };

            SendAuthoritativeState(state);
        }

        [ServerRpc]
        protected virtual void SendAuthoritativeState(NetworkCharacterReplicatedState state)
        {
            BroadcastAuthoritativeState(state);
        }

        [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
        protected virtual void BroadcastAuthoritativeState(NetworkCharacterReplicatedState state)
        {
            bool hadRemoteState = _hasRemoteState;
            NetworkCharacterReplicatedState previousState = _remoteState;
            _remoteState = state;
            _hasRemoteState = true;

            if (IsOwner)
            {
                return;
            }

            if (hadRemoteState)
            {
                HandleRemoteStepTransitions(previousState, state);
            }
        }

        protected virtual void ClearInputs()
        {
            _moveInput = Vector2.zero;
            _deltaLookInput = Vector2.zero;
            _isSprintPressed = false;
            _wantsToJump = false;
        }

        public virtual ICharacterInteraction GetActiveInteraction()
        {
            return _interaction;
        }

        protected virtual NetworkObject GetActiveInteractionNetworkObject()
        {
            Component interactionComponent = _interaction as Component;
            if (interactionComponent == null && _interactionCollider != null)
            {
                interactionComponent = _interactionCollider.GetComponent<ICharacterInteraction>() as Component;
            }

            if (interactionComponent == null)
            {
                return null;
            }

            return interactionComponent.GetComponent<NetworkObject>();
        }

        protected virtual ICharacterInteraction ResolveInteraction(NetworkObject interactionObject)
        {
            if (interactionObject == null)
            {
                return null;
            }

            return interactionObject.GetComponent<ICharacterInteraction>();
        }

        protected virtual void ApplyStepModifier(StepModifierSettings modifierSettings)
        {
            if (modifierSettings == null)
            {
                return;
            }

            if (proceduralAnimation == null)
            {
                proceduralAnimation = GetComponent<ProceduralAnimationComponent>();
                if (proceduralAnimation == null)
                {
                    proceduralAnimation = GetComponentInChildren<ProceduralAnimationComponent>(true);
                }
            }

            if (proceduralAnimation == null)
            {
                return;
            }

            // proceduralAnimation.UpdateAnimationModifier(modifierSettings);
        }

        protected virtual void OnMovementChange(bool isMoving)
        {
            if (!_isGrounded)
            {
                return;
            }

            ApplyStepModifier(isMoving ? startMoving : stopMoving);
        }

        protected virtual void HandleRemoteStepTransitions(
            NetworkCharacterReplicatedState previousState,
            NetworkCharacterReplicatedState currentState)
        {
            if (!currentState.IsGrounded)
            {
                return;
            }

            bool previousMoving = HasMoveInputs(previousState.MoveInput);
            bool currentMoving = HasMoveInputs(currentState.MoveInput);

            if (!currentMoving && previousState.IsCrouching != currentState.IsCrouching)
            {
                ApplyStepModifier(currentState.IsCrouching ? stepCrouch : stepUncrouch);
                return;
            }

            if (previousMoving != currentMoving)
            {
                ApplyStepModifier(currentMoving ? startMoving : stopMoving);
            }
        }

        protected virtual void SyncProceduralData()
        {
            if (proceduralDataBehaviour == null)
            {
                proceduralDataBehaviour = GetComponent<CASProceduralDataBehaviour>();
                if (proceduralDataBehaviour == null)
                {
                    proceduralDataBehaviour = GetComponentInChildren<CASProceduralDataBehaviour>(true);
                }
            }

            if (proceduralDataBehaviour == null || proceduralDataBehaviour.proceduralData == null)
            {
                return;
            }

            CASProceduralData proceduralData = proceduralDataBehaviour.proceduralData;
            proceduralData.Gait = _animatorGait;
            proceduralData.LookInput = _lookInput;
            proceduralData.DeltaLookInput = IsOwner ? _deltaLookInput : Vector2.zero;
            proceduralData.MoveInput = _moveInput;
            proceduralData.IsGrounded = IsGrounded;
            proceduralData.RightHandIkTarget = RightHandIkTarget;
            proceduralData.LeftHandIkTarget = LeftHandIkTarget;
            proceduralData.GetLeftHandTarget = _interaction?.GetLeftHandTarget();
            proceduralData.GetLeftInteractionHandTarget = _interaction?.GetLeftHandTarget();
        }

        protected virtual void UpdateAnimatorParameters()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator == null)
            {
                return;
            }

            float gaitAlpha = animatorGaitSmoothing <= 0f
                ? 1f
                : 1f - Mathf.Exp(-animatorGaitSmoothing * Time.deltaTime);
            float crouchAlpha = animatorCrouchSmoothing <= 0f
                ? 1f
                : 1f - Mathf.Exp(-animatorCrouchSmoothing * Time.deltaTime);

            _animatorGait = Mathf.Lerp(_animatorGait, _gait, gaitAlpha);
            _animatorCrouchWeight = Mathf.Lerp(_animatorCrouchWeight, _isCrouching ? 1f : 0f, crouchAlpha);

            Vector2 moveInput = GetAnimatorMoveInput();
            bool isAnimatorMoving = moveInput.sqrMagnitude > movingVelocityThreshold * movingVelocityThreshold;
            if (!isFirstPerson && orientRotationToMovement)
            {
                moveInput.y = Mathf.Clamp01(moveInput.magnitude);
                moveInput.x = 0f;
            }

            float moveAlpha = animatorMoveSmoothing <= 0f
                ? 1f
                : 1f - Mathf.Exp(-animatorMoveSmoothing * Time.deltaTime);
            Vector2 animatorMove = Vector2.Lerp(
                new Vector2(animator.GetFloat(Animator_MoveX), animator.GetFloat(Animator_MoveY)),
                moveInput,
                moveAlpha);

            float viewWeight = characterCamera != null ? characterCamera.ViewWeight : (isFirstPerson ? 1f : 0f);

            animator.SetFloat(Animator_MoveX, animatorMove.x);
            animator.SetFloat(Animator_MoveY, animatorMove.y);
            animator.SetFloat(Animator_Gait, _animatorGait);
            animator.SetFloat(Animator_ViewWeight, viewWeight);
            animator.SetFloat(Animator_CrouchWeight, _animatorCrouchWeight);
            animator.SetBool(Animator_Crouch, _isCrouching);
            animator.SetBool(Animator_IsInAir, _isInAir);
            animator.SetBool(Animator_IsFirstPerson, isFirstPerson);
            animator.SetBool(Animator_IsMoving, isAnimatorMoving);
        }

        protected virtual Vector2 GetAnimatorMoveInput()
        {
            if (IsOwner)
            {
                Vector3 localDesiredVelocity = transform.InverseTransformDirection(_desiredPlanarVelocity);
                float desiredVelocityScale = Mathf.Max(0.01f, _activeGait.velocity);

                Vector2 normalizedDesiredVelocity = new Vector2(
                    localDesiredVelocity.x / desiredVelocityScale,
                    localDesiredVelocity.z / desiredVelocityScale
                );

                // Square normalization (fix for diagonal 0.7 issue)
                float maxComponent = Mathf.Max(
                    Mathf.Abs(normalizedDesiredVelocity.x),
                    Mathf.Abs(normalizedDesiredVelocity.y),
                    0.0001f
                );

                normalizedDesiredVelocity /= maxComponent;

                return new Vector2(
                    Mathf.Clamp(normalizedDesiredVelocity.x, -1f, 1f),
                    Mathf.Clamp(normalizedDesiredVelocity.y, -1f, 1f)
                );
            }

            Vector3 currentPosition = transform.position;
            if (!_hasAnimatorSamplePosition)
            {
                _lastAnimatorSamplePosition = currentPosition;
                _hasAnimatorSamplePosition = true;
                return Vector2.zero;
            }

            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 worldVelocity = (currentPosition - _lastAnimatorSamplePosition) / deltaTime;
            _lastAnimatorSamplePosition = currentPosition;

            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);
            float velocityScale = Mathf.Max(0.01f, _activeGait.velocity);

            Vector2 normalizedVelocity = new Vector2(
                localVelocity.x / velocityScale,
                localVelocity.z / velocityScale
            );

            // Square normalization (same fix here)
            float maxComponentNonOwner = Mathf.Max(
                Mathf.Abs(normalizedVelocity.x),
                Mathf.Abs(normalizedVelocity.y),
                0.0001f
            );

            normalizedVelocity /= maxComponentNonOwner;

            return new Vector2(
                Mathf.Clamp(normalizedVelocity.x, -1f, 1f),
                Mathf.Clamp(normalizedVelocity.y, -1f, 1f)
            );
        }

        protected virtual void ToggleCrouch()
        {
            if (_isSprintPressed)
            {
                _isSprintPressed = false;
            }

            bool wantsToCrouch = !_isCrouching;
            if (!wantsToCrouch && !CanUnCrouch())
            {
                return;
            }

            _isCrouching = wantsToCrouch;

            if (characterCamera != null)
            {
                characterCamera.isCrouching = _isCrouching;
            }

            if (_isGrounded && !HasMoveInputs())
            {
                ApplyStepModifier(_isCrouching ? stepCrouch : stepUncrouch);
            }

            if (_isCrouching)
            {
                Crouch();
                return;
            }

            UnCrouch();
        }

        protected virtual void Crouch()
        {
            float crouchedHeight = _originalHeight * 0.5f;
            float heightDifference = _originalHeight - crouchedHeight;
            _characterController.height = crouchedHeight;

            Vector3 crouchedCenter = _originalCenter;
            crouchedCenter.y -= heightDifference / 2f;
            _characterController.center = crouchedCenter;
        }

        protected virtual void UnCrouch()
        {
            _characterController.height = _originalHeight;
            _characterController.center = _originalCenter;
        }

        protected virtual bool CanUnCrouch()
        {
            float height = _originalHeight - _characterController.radius * 2f;
            Vector3 position = transform.TransformPoint(_originalCenter + Vector3.up * height / 2f);
            return !Physics.CheckSphere(position, _characterController.radius, groundLayer, QueryTriggerInteraction.Ignore);
        }

        protected virtual bool HasMoveInputs()
        {
            return HasMoveInputs(_moveInput);
        }

        protected virtual bool HasMoveInputs(Vector2 moveInput)
        {
            return moveInput.sqrMagnitude > 0f;
        }

        protected virtual float GetHorizontalSpeed()
        {
            return new Vector2(_velocity.x, _velocity.z).magnitude;
        }

        protected virtual NetworkCharacterMovementState GetDesiredGroundedState()
        {
            if (_isCrouching)
            {
                return NetworkCharacterMovementState.CrouchWalk;
            }

            if (_isSprintPressed && CanSprint())
            {
                return NetworkCharacterMovementState.Sprint;
            }

            if (_moveInput.magnitude <= walkInputThreshold)
            {
                return NetworkCharacterMovementState.Walk;
            }

            return useDefaultWalk ? NetworkCharacterMovementState.Walk : NetworkCharacterMovementState.Jog;
        }

        protected virtual bool CanSprint()
        {
            return _moveInput.y > 0f || (!isFirstPerson && orientRotationToMovement);
        }

        protected virtual NetworkMovementGaitSettings GetGaitSettings(NetworkCharacterMovementState movementState)
        {
            switch (movementState)
            {
                case NetworkCharacterMovementState.Sprint:
                    return sprintGait;
                case NetworkCharacterMovementState.Jog:
                    return jogGait;
                case NetworkCharacterMovementState.Walk:
                    return walkGait;
                case NetworkCharacterMovementState.CrouchWalk:
                    return crouchWalkGait;
                default:
                    return default;
            }
        }

        protected virtual void UpdateMovementState()
        {
            bool wasGrounded = _isGrounded;
            bool wasInAir = _isInAir;
            _surfaceTrace = TraceSurfaceBelow();
            _isGrounded = ResolveGroundedState(_surfaceTrace);

            if (_isGrounded)
            {
                _timeSinceLastGrounded = 0f;
            }
            else
            {
                _timeSinceLastGrounded += Time.deltaTime;
            }

            bool canJumpNow = _isGrounded || _timeSinceLastGrounded <= jumpCoyoteTime;
            if (_wantsToJump && canJumpNow)
            {
                _velocity.y = Mathf.Sqrt(Mathf.Max(0f, 2f * gravity * jumpHeight));
                _isGrounded = false;
                _isInAir = true;
            }

            _wantsToJump = false;

            if (_isGrounded)
            {
                if (!wasGrounded && wasInAir)
                {
                    _isInAir = false;
                    ApplyLandingMomentum();
                }
            }
            else if (!_isInAir && ShouldEnterInAirState(_surfaceTrace))
            {
                _isInAir = true;
            }

            _prevMovementState = _movementState;

            if (!HasMoveInputs())
            {
                _movementState = NetworkCharacterMovementState.Idle;
                return;
            }

            _movementState = GetDesiredGroundedState();
        }

        protected virtual void UpdateGait()
        {
            if (_prevMovementState != _movementState)
            {
                _activeGait = GetGaitSettings(_movementState);

                if (_movementState == NetworkCharacterMovementState.Idle)
                {
                    _activeGait.deceleration = idleDeceleration;
                }
                else if (_prevMovementState > _movementState)
                {
                    _activeGait.deceleration = GetGaitSettings(_prevMovementState).deceleration;
                }
            }

            float speed = GetHorizontalSpeed();
            float walkSpeed = Mathf.Max(0.01f, walkGait.velocity);
            float jogSpeed = Mathf.Max(walkSpeed + 0.01f, jogGait.velocity);
            float sprintSpeed = Mathf.Max(jogSpeed + 0.01f, sprintGait.velocity);

            if (speed <= walkSpeed)
            {
                _gait = _movementState == NetworkCharacterMovementState.Idle ? 0f : 1f;
            }
            else if (speed <= jogSpeed)
            {
                _gait = 1f + Mathf.InverseLerp(walkSpeed, jogSpeed, speed);
            }
            else
            {
                _gait = 2f + Mathf.InverseLerp(jogSpeed, sprintSpeed, Mathf.Min(speed, sprintSpeed));
            }

            _gait = Mathf.Lerp(_networkGait, _gait, gaitSmoothing <= 0f ? 1f : 1f - Mathf.Exp(-gaitSmoothing * Time.deltaTime));
            _networkGait = _gait;
        }

        protected virtual void UpdateRotation()
        {
            _lookInput.x = Mathf.Clamp(_lookInput.x + _deltaLookInput.x, -90f, 90f);
            _lookInput.y = Mathf.Clamp(_lookInput.y - _deltaLookInput.y, -90f, 90f);

            _aimRotation *= Quaternion.Euler(0f, _deltaLookInput.x, 0f);
            _aimRotation.Normalize();

            if (characterCamera != null)
            {
                characterCamera.pitchInput = _lookInput.y;
                characterCamera.yawInput = _aimRotation.eulerAngles.y;
                characterCamera.isFirstPerson = isFirstPerson;
            }

            if(_characterController.enabled == false) return;
            
            if (isFirstPerson)
            {
                SetCharacterSmoothRotation(_aimRotation, 20f);
                return;
            }

            if (HasMoveInputs())
            {
                Quaternion targetRotation = orientRotationToMovement
                    ? Quaternion.LookRotation(GetMovementInputDirection(), transform.up)
                    : _aimRotation;
                SetCharacterSmoothRotation(targetRotation, _activeGait.rotationSpeed);
            }
        }

        protected virtual void SetCharacterSmoothRotation(Quaternion targetRotation, float speed)
        {
            _targetRotation = targetRotation;

            if (Mathf.Approximately(speed, 0f))
            {
                transform.rotation = targetRotation;
                return;
            }

            float alpha = 1f - Mathf.Exp(-speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alpha);
        }

        protected virtual void UpdateMovement()
        {
            if (_isInAir)
            {
                _velocity.y -= gravity * Time.deltaTime;
                _velocity.y = Mathf.Max(_velocity.y, -maxFallVelocity);

                Vector3 horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
                float currentHorizontalSpeed = horizontalVelocity.magnitude;
                Vector3 airInput = GetMovementInputDirection() * (airAcceleration * Time.deltaTime);
                horizontalVelocity += airInput;
                float horizontalCap = Mathf.Max(airMaxSpeed, currentHorizontalSpeed);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, horizontalCap);
                _desiredPlanarVelocity = horizontalVelocity;
                _velocity.x = horizontalVelocity.x;
                _velocity.z = horizontalVelocity.z;
            }
            else
            {
                _velocity.y = -Mathf.Max(0f, groundedStickVelocity);

                Vector3 targetVelocity = Vector3.zero;
                if (HasMoveInputs())
                {
                    Vector3 direction = GetSlopeAdjustedVelocity(GetMovementInputDirection());
                    targetVelocity = direction * _activeGait.velocity;
                }

                _desiredPlanarVelocity = targetVelocity;

                Vector3 horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
                float interpSpeed = targetVelocity.sqrMagnitude > horizontalVelocity.sqrMagnitude
                    ? _activeGait.acceleration
                    : _activeGait.deceleration;
                float alpha = interpSpeed <= 0f ? 1f : 1f - Mathf.Exp(-interpSpeed * Time.deltaTime);
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, alpha);
                _velocity.x = horizontalVelocity.x;
                _velocity.z = horizontalVelocity.z;
            }
            if(_characterController.enabled)
                _characterController.Move(_velocity * Time.deltaTime);
        }

        protected virtual void ApplyLandingMomentum()
        {
            Vector3 landingVelocity = _velocity;
            landingVelocity.y = 0f;
            landingVelocity *= landingMomentumFactor;

            if (!HasMoveInputs())
            {
                landingVelocity = Vector3.zero;
            }

            _velocity.x = landingVelocity.x;
            _velocity.z = landingVelocity.z;
        }

        protected virtual NetworkSurfaceTraceInfo TraceSurfaceBelow()
        {
            NetworkSurfaceTraceInfo traceInfo = default;
            float probeRadius = Mathf.Max(0.01f, _characterController.radius * groundProbeRadiusFactor);
            float halfHeight = Mathf.Max(_characterController.height * 0.5f, _characterController.radius);
            float hemisphereOffset = Mathf.Max(0f, halfHeight - _characterController.radius);
            Vector3 bottom = transform.TransformPoint(_characterController.center + Vector3.down * hemisphereOffset);
            Vector3 top = transform.TransformPoint(_characterController.center + Vector3.up * hemisphereOffset);

            bool foundHit = Physics.CapsuleCast(bottom, top, probeRadius, Vector3.down, out RaycastHit hitInfo,
                groundSnapDistance, groundLayer, QueryTriggerInteraction.Ignore);
            if (!foundHit)
            {
                return traceInfo;
            }

            traceInfo.hasHit = true;
            traceInfo.hitInfo = hitInfo;
            float distanceFromBottom = Mathf.Max(0f, hitInfo.distance);
            bool isStableSlope = Vector3.Angle(hitInfo.normal, transform.up) <= maxStableSlopeAngle;
            bool withinSnapDistance = distanceFromBottom <= groundSnapDistance;
            bool withinSnapSpeed = _velocity.y <= -Mathf.Max(0f, groundedStickVelocity);
            traceInfo.canSnapToGround = isStableSlope && withinSnapDistance && withinSnapSpeed;
            return traceInfo;
        }

        protected virtual bool ResolveGroundedState(NetworkSurfaceTraceInfo surfaceTrace)
        {
            return _characterController.isGrounded || surfaceTrace.canSnapToGround;
        }

        protected virtual bool ShouldEnterInAirState(NetworkSurfaceTraceInfo surfaceTrace)
        {
            if (_timeSinceLastGrounded < fallDelay)
            {
                return false;
            }

            if (surfaceTrace.hasHit && surfaceTrace.canSnapToGround)
            {
                return false;
            }

            float distanceFromBottom = surfaceTrace.hasHit ? Mathf.Max(0f, surfaceTrace.hitInfo.distance) : float.PositiveInfinity;
            bool isStableSlope = surfaceTrace.hasHit &&
                Vector3.Angle(surfaceTrace.hitInfo.normal, transform.up) <= maxStableSlopeAngle;
            bool hasStableSurfaceNearby = surfaceTrace.hasHit && isStableSlope &&
                distanceFromBottom <= groundSnapDistance && _velocity.y <= 0f;
            if (hasStableSurfaceNearby)
            {
                return false;
            }

            if (_velocity.y <= -minFallSpeedToInAir)
            {
                return true;
            }

            bool stillTooCloseToGround = surfaceTrace.hasHit && distanceFromBottom <= groundSnapDistance;
            return !stillTooCloseToGround;
        }

        protected virtual Vector3 GetSlopeAdjustedVelocity(Vector3 velocity)
        {
            if (!_isGrounded || velocity.sqrMagnitude < 0.001f || !_surfaceTrace.hasHit)
            {
                return velocity;
            }

            return Vector3.ProjectOnPlane(velocity, _surfaceTrace.hitInfo.normal).normalized * velocity.magnitude;
        }

        protected virtual Vector3 GetMovementInputDirection()
        {
            Vector2 input = _moveInput.normalized;
            return _aimRotation * Vector3.forward * input.y + _aimRotation * Vector3.right * input.x;
        }

        public virtual void SetMoveInput(Vector2 moveInput)
        {
            if (!IsOwner)
            {
                return;
            }

            _moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        }

        public virtual void SetLookInput(Vector2 lookInput)
        {
            if (!IsOwner)
            {
                return;
            }

            _deltaLookInput = lookSensitivity * Time.deltaTime * lookInput;
        }

        public virtual void SetSprintInput(bool pressed)
        {
            if (!IsOwner)
            {
                return;
            }

            _isSprintPressed = pressed;
        }

        public virtual void SetJumpInput()
        {
            if (!IsOwner)
            {
                return;
            }

            if (_isInAir && _timeSinceLastGrounded > jumpCoyoteTime)
            {
                return;
            }

            _wantsToJump = true;
        }

        public virtual void SetInteractInput()
        {
            Debug.Log("Interact input received");
            if (!IsOwner)
            {
                Debug.LogError("SetInteractInput called on non-owner client. Ignoring input.");
                return;
            }

            if (_interaction == null && _interactionCollider != null)
            {
                Debug.Log("No active interaction, but have interaction collider. Attempting to get interaction component.");
                _interaction = _interactionCollider.GetComponent<ICharacterInteraction>();
            }
            Debug.Log($"Current interaction: {_interaction}");
            _interaction?.StartInteraction(gameObject);
            Debug.Log("Interaction input processed");
        }

        public virtual void ToggleCrouchInput()
        {
            if (!IsOwner)
            {
                return;
            }

            ToggleCrouch();
        }

        public virtual void ToggleWalkMode()
        {
            if (!IsOwner)
            {
                return;
            }

            useDefaultWalk = !useDefaultWalk;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            _interactionCollider = other;
            _interaction = other.GetComponent<ICharacterInteraction>();
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other != _interactionCollider)
            {
                return;
            }

            _interactionCollider = null;
            _interaction = null;
        }

#if UNITY_INPUT_SYSTEM_ENABLED
        protected virtual void RefreshInputSubscriptions()
        {
            if (!IsClientInitialized || !isActiveAndEnabled || !IsOwner)
            {
                DisableInputSubscriptions();
                return;
            }

            if (_inputSubscriptionsActive)
            {
                return;
            }

            SubscribeAction(moveInputAction, OnMovePerformed, OnMoveCanceled);
            SubscribeAction(lookInputAction, OnLookPerformed, OnLookCanceled);
            SubscribeAction(sprintInputAction, OnSprintPerformed, OnSprintCanceled);
            SubscribeAction(jumpInputAction, OnJumpPerformed, null);
            SubscribeAction(interactInputAction, OnInteractPerformed, null);
            SubscribeAction(crouchInputAction, OnCrouchPerformed, null);
            SubscribeAction(toggleWalkInputAction, OnToggleWalkPerformed, null);
            _inputSubscriptionsActive = true;
        }

        protected virtual void DisableInputSubscriptions()
        {
            if (!_inputSubscriptionsActive)
            {
                return;
            }

            UnsubscribeAction(moveInputAction, OnMovePerformed, OnMoveCanceled);
            UnsubscribeAction(lookInputAction, OnLookPerformed, OnLookCanceled);
            UnsubscribeAction(sprintInputAction, OnSprintPerformed, OnSprintCanceled);
            UnsubscribeAction(jumpInputAction, OnJumpPerformed, null);
            UnsubscribeAction(interactInputAction, OnInteractPerformed, null);
            UnsubscribeAction(crouchInputAction, OnCrouchPerformed, null);
            UnsubscribeAction(toggleWalkInputAction, OnToggleWalkPerformed, null);
            _inputSubscriptionsActive = false;
        }

        protected virtual void SubscribeAction(InputActionReference actionReference,
            Action<InputAction.CallbackContext> performedHandler,
            Action<InputAction.CallbackContext> canceledHandler)
        {
            if (actionReference == null || actionReference.action == null)
            {
                return;
            }

            actionReference.action.Enable();
            actionReference.action.performed += performedHandler;
            if (canceledHandler != null)
            {
                actionReference.action.canceled += canceledHandler;
            }
        }

        protected virtual void UnsubscribeAction(InputActionReference actionReference,
            Action<InputAction.CallbackContext> performedHandler,
            Action<InputAction.CallbackContext> canceledHandler)
        {
            if (actionReference == null || actionReference.action == null)
            {
                return;
            }

            actionReference.action.performed -= performedHandler;
            if (canceledHandler != null)
            {
                actionReference.action.canceled -= canceledHandler;
            }
        }

        protected virtual void OnMovePerformed(InputAction.CallbackContext context)
        {
            SetMoveInput(context.ReadValue<Vector2>());
        }

        protected virtual void OnMoveCanceled(InputAction.CallbackContext context)
        {
            SetMoveInput(Vector2.zero);
        }

        protected virtual void OnLookPerformed(InputAction.CallbackContext context)
        {
            SetLookInput(context.ReadValue<Vector2>());
        }

        protected virtual void OnLookCanceled(InputAction.CallbackContext context)
        {
            SetLookInput(Vector2.zero);
        }

        protected virtual void OnSprintPerformed(InputAction.CallbackContext context)
        {
            SetSprintInput(context.ReadValueAsButton());
        }

        protected virtual void OnSprintCanceled(InputAction.CallbackContext context)
        {
            SetSprintInput(false);
        }

        protected virtual void OnJumpPerformed(InputAction.CallbackContext context)
        {
            SetJumpInput();
        }

        protected virtual void OnInteractPerformed(InputAction.CallbackContext context)
        {
            SetInteractInput();
        }

        protected virtual void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            ToggleCrouchInput();
        }

        protected virtual void OnToggleWalkPerformed(InputAction.CallbackContext context)
        {
            ToggleWalkMode();
        }
#endif
    }
}