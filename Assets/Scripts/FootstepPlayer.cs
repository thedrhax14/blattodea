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

    private Vector3 _lastPosition;
    private float _distanceSinceStep;
    private float _timeSinceStep;
    private int _lastClipIndex = -1;

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

        _lastPosition = target.position;
    }

    private void OnEnable()
    {
        if (target == null)
        {
            target = transform;
        }

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

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
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