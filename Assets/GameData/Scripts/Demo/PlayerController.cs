using UnityEngine;
using UnityEngine.InputSystem;

namespace com.blattodea.core.demo {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference jumpAction;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float cameraHeight = 1.65f;
        [SerializeField] private float lookSensitivity = 60f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Head Bob")]
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private AnimationCurve bobHorizontalCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -1f),
            new Keyframe(1f, 0f));
        [SerializeField] private AnimationCurve bobVerticalCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, 1f),
            new Keyframe(1f, 0f));
        [SerializeField] private Vector2 bobAmplitude = new Vector2(0.018f, 0.05f);
        [SerializeField] private float bobSprintMultiplier = 1.35f;
        [SerializeField] private float bobReturnSpeed = 10f;

        [Header("Footsteps")]
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] [Range(0f, 1f)] private float footstepVolume = 0.85f;
        [SerializeField] [Range(0f, 0.25f)] private float footstepPitchVariation = 0.05f;

        [Header("Jump Audio")]
        [SerializeField] private AudioSource jumpAudioSource;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip landClip;
        [SerializeField] [Range(0f, 1f)] private float jumpVolume = 0.9f;
        [SerializeField] [Range(0f, 1f)] private float landVolume = 1f;

        private CharacterController characterController;
        private Vector3 planarVelocity;
        private Vector3 baseCameraLocalOffset;
        private Vector3 currentCameraBobOffset;
        private float verticalVelocity;
        private float pitch;
        private float bobTimer;
        private int lastStepPhase = -1;
        private int lastFootstepIndex = -1;
        private bool wasGrounded;

        private bool moveActionOwned;
        private bool lookActionOwned;
        private bool sprintActionOwned;
        private bool jumpActionOwned;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            ResolveCamera();
            CacheCameraOffset();
            EnsureFootstepSource();
            EnsureJumpAudioSource();
            ApplyCameraTransform();
            wasGrounded = characterController.isGrounded;
        }

        private void OnEnable()
        {
            cameraTransform = Camera.main != null ? Camera.main.transform : cameraTransform;
            moveActionOwned = EnableActionIfNeeded(moveAction);
            lookActionOwned = EnableActionIfNeeded(lookAction);
            sprintActionOwned = EnableActionIfNeeded(sprintAction);
            jumpActionOwned = EnableActionIfNeeded(jumpAction);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            DisableActionIfOwned(moveAction, moveActionOwned);
            DisableActionIfOwned(lookAction, lookActionOwned);
            DisableActionIfOwned(sprintAction, sprintActionOwned);
            DisableActionIfOwned(jumpAction, jumpActionOwned);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleCameraEffects();
        }

        private void HandleLook()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector2 lookInput = ReadVector2(lookAction);
            float lookDelta = lookSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up, lookInput.x * lookDelta);

            pitch = Mathf.Clamp(pitch - (lookInput.y * lookDelta), minPitch, maxPitch);
            ApplyCameraTransform();
        }

        private void HandleMovement()
        {
            bool startedGrounded = characterController.isGrounded;
            Vector2 moveInput = ReadVector2(moveAction);
            Vector3 inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            if (inputDirection.sqrMagnitude > 1f)
            {
                inputDirection.Normalize();
            }

            float targetSpeed = IsSprinting() ? sprintSpeed : walkSpeed;
            Vector3 targetPlanarVelocity = inputDirection * targetSpeed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, targetPlanarVelocity, acceleration * Time.deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (startedGrounded && WasPressedThisFrame(jumpAction))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                PlayJumpSound();
                wasGrounded = false;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = planarVelocity + (Vector3.up * verticalVelocity);
            CollisionFlags collisionFlags = characterController.Move(velocity * Time.deltaTime);

            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            if ((collisionFlags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            bool endedGrounded = (collisionFlags & CollisionFlags.Below) != 0 || characterController.isGrounded;
            if (!wasGrounded && endedGrounded)
            {
                PlayLandingSound();
            }

            wasGrounded = endedGrounded;
        }

        private void HandleCameraEffects()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0f;

            bool grounded = characterController.isGrounded;
            bool isMoving = grounded && horizontalVelocity.sqrMagnitude > 0.01f;

            Vector3 targetBobOffset = Vector3.zero;
            if (isMoving)
            {
                float bobSpeedMultiplier = IsSprinting() ? bobSprintMultiplier : 1f;
                float speedFactor = Mathf.Clamp01(horizontalVelocity.magnitude / Mathf.Max(walkSpeed, 0.01f));

                bobTimer += Time.deltaTime * bobFrequency * bobSpeedMultiplier * Mathf.Lerp(0.85f, 1.25f, speedFactor);

                float bobCycle = Mathf.Repeat(bobTimer / (Mathf.PI * 2f), 1f);
                targetBobOffset = new Vector3(
                    bobHorizontalCurve.Evaluate(bobCycle) * bobAmplitude.x,
                    bobVerticalCurve.Evaluate(bobCycle) * bobAmplitude.y,
                    0f);

                int stepPhase = Mathf.FloorToInt(bobCycle * 2f);
                if (stepPhase != lastStepPhase)
                {
                    lastStepPhase = stepPhase;
                    PlayFootstep();
                }
            }
            else
            {
                float bobCycle = Mathf.Repeat(bobTimer / (Mathf.PI * 2f), 1f);
                lastStepPhase = Mathf.FloorToInt(bobCycle * 2f);
            }

            currentCameraBobOffset = Vector3.Lerp(
                currentCameraBobOffset,
                targetBobOffset,
                Time.deltaTime * (isMoving ? bobReturnSpeed * 0.5f : bobReturnSpeed));

            ApplyCameraTransform();
        }

        private void ApplyCameraTransform()
        {
            if (cameraTransform == null)
            {
                return;
            }

            cameraTransform.SetPositionAndRotation(
                transform.TransformPoint(baseCameraLocalOffset + currentCameraBobOffset),
                Quaternion.Euler(pitch, transform.eulerAngles.y, 0f));
        }

        private void ResolveCamera()
        {
            if (cameraTransform != null)
            {
                return;
            }

            Camera childCamera = GetComponentInChildren<Camera>(true);
            if (childCamera != null)
            {
                cameraTransform = childCamera.transform;
                return;
            }

            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void CacheCameraOffset()
        {
            if (cameraTransform == null)
            {
                baseCameraLocalOffset = new Vector3(0f, cameraHeight, 0f);
                return;
            }

            baseCameraLocalOffset = transform.InverseTransformPoint(cameraTransform.position);
            if (baseCameraLocalOffset.sqrMagnitude < 0.0001f)
            {
                baseCameraLocalOffset = new Vector3(0f, cameraHeight, 0f);
            }

            pitch = NormalizeAngle(cameraTransform.eulerAngles.x);
        }

        private void EnsureFootstepSource()
        {
            if (footstepSource != null)
            {
                return;
            }

            footstepSource = GetComponent<AudioSource>();
            if (footstepSource == null && footstepClips != null && footstepClips.Length > 0)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
                footstepSource.playOnAwake = false;
                footstepSource.spatialBlend = 0f;
            }
        }

        private void EnsureJumpAudioSource()
        {
            if (jumpAudioSource != null)
            {
                return;
            }

            jumpAudioSource = footstepSource != null ? footstepSource : GetComponent<AudioSource>();
            if (jumpAudioSource == null && (jumpClip != null || landClip != null))
            {
                jumpAudioSource = gameObject.AddComponent<AudioSource>();
                jumpAudioSource.playOnAwake = false;
                jumpAudioSource.spatialBlend = 0f;
            }
        }

        private void PlayFootstep()
        {
            if (footstepSource == null || footstepClips == null || footstepClips.Length == 0)
            {
                return;
            }

            int clipIndex = Random.Range(0, footstepClips.Length);
            if (footstepClips.Length > 1 && clipIndex == lastFootstepIndex)
            {
                clipIndex = (clipIndex + 1) % footstepClips.Length;
            }

            lastFootstepIndex = clipIndex;
            footstepSource.pitch = 1f + Random.Range(-footstepPitchVariation, footstepPitchVariation);
            footstepSource.PlayOneShot(footstepClips[clipIndex], footstepVolume);
        }

        private void PlayJumpSound()
        {
            PlayOneShot(jumpClip, jumpVolume);
        }

        private void PlayLandingSound()
        {
            PlayOneShot(landClip, landVolume);
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (jumpAudioSource == null || clip == null)
            {
                return;
            }

            jumpAudioSource.pitch = 1f;
            jumpAudioSource.PlayOneShot(clip, volume);
        }

        private bool IsSprinting()
        {
            return sprintAction != null && sprintAction.action != null && sprintAction.action.IsPressed();
        }

        private static bool WasPressedThisFrame(InputActionReference actionReference)
        {
            return actionReference != null && actionReference.action != null && actionReference.action.WasPressedThisFrame();
        }

        private static Vector2 ReadVector2(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null)
            {
                return Vector2.zero;
            }

            return actionReference.action.ReadValue<Vector2>();
        }

        private static bool EnableActionIfNeeded(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null || actionReference.action.enabled)
            {
                return false;
            }

            actionReference.action.Enable();
            return true;
        }

        private static void DisableActionIfOwned(InputActionReference actionReference, bool owned)
        {
            if (!owned || actionReference == null || actionReference.action == null)
            {
                return;
            }

            actionReference.action.Disable();
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f)
            {
                angle -= 360f;
            }

            while (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }
    }
}