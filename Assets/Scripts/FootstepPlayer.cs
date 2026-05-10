using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class FootstepPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Detection")]
    [SerializeField] private bool usePlanarMovement = true;
    [SerializeField] private float minVelocity = 0.1f;

    [Header("Distance Trigger")]
    [SerializeField] private bool useDistanceTrigger = true;
    [SerializeField] private float distancePerStep = 2f;

    [Header("Velocity Trigger")]
    [SerializeField] private bool useVelocityTrigger;
    [SerializeField] private float velocityForMaxRate = 6f;
    [SerializeField] private float minStepInterval = 0.55f;
    [SerializeField] private float maxStepInterval = 0.25f;

    [Header("Variation")]
    [SerializeField] private Vector2 volumeRange = new Vector2(0.9f, 1f);
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private int concurrentStepVoices = 3;

    private Vector3 _lastPosition;
    private float _distanceSinceStep;
    private float _timeSinceStep;
    private int _lastClipIndex = -1;
    private AudioSource[] _playbackSources;
    private int _nextPlaybackSourceIndex;

    private void Reset()
    {
        target = transform;
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        EnsurePlaybackSources();
        _lastPosition = target.position;
    }

    private void OnEnable()
    {
        if (target == null)
        {
            target = transform;
        }

        EnsurePlaybackSources();
        _lastPosition = target.position;
        _distanceSinceStep = 0f;
        _timeSinceStep = 0f;
    }

    private void Update()
    {
        if (target == null || audioSource == null || footstepClips == null || footstepClips.Length == 0)
        {
            return;
        }

        Vector3 currentPosition = target.position;
        Vector3 delta = currentPosition - _lastPosition;
        _lastPosition = currentPosition;

        if (usePlanarMovement)
        {
            delta.y = 0f;
        }

        float movedDistance = delta.magnitude;
        float speed = GetCurrentSpeed(movedDistance);
        _timeSinceStep += Time.deltaTime;

        if (speed < minVelocity)
        {
            _distanceSinceStep = 0f;
            return;
        }

        _distanceSinceStep += movedDistance;

        bool shouldPlayFromDistance = useDistanceTrigger && distancePerStep > 0f && _distanceSinceStep >= distancePerStep;
        bool shouldPlayFromVelocity = useVelocityTrigger && _timeSinceStep >= GetVelocityStepInterval(speed);

        if (!shouldPlayFromDistance && !shouldPlayFromVelocity)
        {
            return;
        }

        PlayStep();
        _distanceSinceStep = 0f;
        _timeSinceStep = 0f;
    }

    private float GetCurrentSpeed(float movedDistance)
    {
        return Time.deltaTime > 0f ? movedDistance / Time.deltaTime : 0f;
    }

    private float GetVelocityStepInterval(float speed)
    {
        if (velocityForMaxRate <= minVelocity)
        {
            return maxStepInterval;
        }

        float t = Mathf.InverseLerp(minVelocity, velocityForMaxRate, speed);
        return Mathf.Lerp(minStepInterval, maxStepInterval, t);
    }

    private void PlayStep()
    {
        int clipIndex = GetNextClipIndex();
        AudioClip clip = footstepClips[clipIndex];

        if (clip == null)
        {
            return;
        }

        AudioSource playbackSource = GetPlaybackSource();
        playbackSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        playbackSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }

    private void EnsurePlaybackSources()
    {
        if (audioSource == null)
        {
            return;
        }

        int voiceCount = Mathf.Max(1, concurrentStepVoices);

        if (_playbackSources != null && _playbackSources.Length == voiceCount)
        {
            for (int i = 0; i < _playbackSources.Length; i++)
            {
                if (_playbackSources[i] == null)
                {
                    _playbackSources[i] = CreatePlaybackSource();
                }
            }

            return;
        }

        _playbackSources = new AudioSource[voiceCount];

        for (int i = 0; i < voiceCount; i++)
        {
            _playbackSources[i] = CreatePlaybackSource();
        }

        _nextPlaybackSourceIndex = 0;
    }

    private AudioSource CreatePlaybackSource()
    {
        AudioSource playbackSource = gameObject.AddComponent<AudioSource>();
        playbackSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        playbackSource.mute = audioSource.mute;
        playbackSource.bypassEffects = audioSource.bypassEffects;
        playbackSource.bypassListenerEffects = audioSource.bypassListenerEffects;
        playbackSource.bypassReverbZones = audioSource.bypassReverbZones;
        playbackSource.priority = audioSource.priority;
        playbackSource.volume = audioSource.volume;
        playbackSource.panStereo = audioSource.panStereo;
        playbackSource.spatialBlend = audioSource.spatialBlend;
        playbackSource.reverbZoneMix = audioSource.reverbZoneMix;
        playbackSource.dopplerLevel = audioSource.dopplerLevel;
        playbackSource.spread = audioSource.spread;
        playbackSource.rolloffMode = audioSource.rolloffMode;
        playbackSource.minDistance = audioSource.minDistance;
        playbackSource.maxDistance = audioSource.maxDistance;
        playbackSource.ignoreListenerVolume = audioSource.ignoreListenerVolume;
        playbackSource.ignoreListenerPause = audioSource.ignoreListenerPause;
        playbackSource.velocityUpdateMode = audioSource.velocityUpdateMode;
        playbackSource.spatialize = audioSource.spatialize;
        playbackSource.spatializePostEffects = audioSource.spatializePostEffects;
        playbackSource.clip = null;
        playbackSource.loop = false;
        playbackSource.playOnAwake = false;
        return playbackSource;
    }

    private AudioSource GetPlaybackSource()
    {
        EnsurePlaybackSources();

        for (int i = 0; i < _playbackSources.Length; i++)
        {
            int sourceIndex = (_nextPlaybackSourceIndex + i) % _playbackSources.Length;
            AudioSource playbackSource = _playbackSources[sourceIndex];

            if (!playbackSource.isPlaying)
            {
                _nextPlaybackSourceIndex = (sourceIndex + 1) % _playbackSources.Length;
                return playbackSource;
            }
        }

        AudioSource fallbackSource = _playbackSources[_nextPlaybackSourceIndex];
        _nextPlaybackSourceIndex = (_nextPlaybackSourceIndex + 1) % _playbackSources.Length;
        return fallbackSource;
    }

    private int GetNextClipIndex()
    {
        if (footstepClips.Length == 1)
        {
            _lastClipIndex = 0;
            return 0;
        }

        int clipIndex = Random.Range(0, footstepClips.Length);

        if (clipIndex == _lastClipIndex)
        {
            clipIndex = (clipIndex + 1) % footstepClips.Length;
        }

        _lastClipIndex = clipIndex;
        return clipIndex;
    }
}