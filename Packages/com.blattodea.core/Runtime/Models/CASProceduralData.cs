using System;
using KINEMATION.CharacterAnimationSystem.Addons.FPS.Scripts.Modifiers;
using KINEMATION.Shared.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace Blattodea.Core.Models
{
    /// <summary>
    /// <para>Shared procedural animation data source used by CAS-facing systems.
    /// Gameplay code can write directly to this model, or in networked scenarios
    /// authoritative network state can update it so procedural animation remains
    /// consistent across clients.</para>
    /// <para>Primitive and compact value data can be synchronized directly. Non-primitive
    /// procedural inputs such as clips, transforms, KTransforms, and WeaponSway
    /// instances should usually be assigned locally by gameplay code after the
    /// authoritative gameplay state is known.</para>
    /// <para>For example, a networked item slot change should synchronize the selected
    /// slot, and the receiving gameplay code should then resolve the corresponding
    /// procedural references and assign them to this model.</para>
    /// </summary>
    [Serializable]
    public sealed class CASProceduralData
    {
        [SerializeField] private float aimingWeight;
        [SerializeField] private float gait;
        [SerializeField] private Vector3 lookInput;
        [SerializeField] private Vector2 deltaLookInput;
        [SerializeField] private Vector2 moveInput;
        [SerializeField] private bool isGrounded;
        [SerializeField] private float cameraViewWeight;
        [SerializeField] private AnimationClip gripPose;
        [SerializeField] private Transform gripIkTarget;
        [SerializeField] private KTransform weaponPose;
        [SerializeField] private KTransform rightHandOffsetValue;
        [SerializeField] private KTransform leftHandOffsetValue;
        [SerializeField] private KTransform weaponPoseThirdPerson;
        [SerializeField] private Vector3 weaponPoseThirdPersonRestPositionValue;
        [SerializeField] private Quaternion weaponPoseThirdPersonRestRotationValue;
        [SerializeField] private Transform scopeAimPoint;
        [SerializeField] private bool isAiming;
        [SerializeField] private WeaponSway aimingSway;
        [SerializeField] private WeaponSway movementSway;
        [SerializeField] private float enableHandRIk;
        [SerializeField] private Transform rightHandIkTarget;
        [SerializeField] private float enableHandLIk;
        [SerializeField] private Transform leftHandIkTarget;
        [SerializeField] private float interactionHandLIk;
        [SerializeField] private Transform getLeftHandTarget;
        [SerializeField] private Transform getLeftInteractionHandTarget;

        public float AimingWeight
        {
            get { return aimingWeight; }
            set { aimingWeight = value; }
        }

        public float Gait
        {
            get { return gait; }
            set { gait = value; }
        }

        public Vector3 LookInput
        {
            get { return lookInput; }
            set { lookInput = value; }
        }

        public Vector2 DeltaLookInput
        {
            get { return deltaLookInput; }
            set { deltaLookInput = value; }
        }

        public Vector2 MoveInput
        {
            get { return moveInput; }
            set { moveInput = value; }
        }

        public bool IsGrounded
        {
            get { return isGrounded; }
            set { isGrounded = value; }
        }

        public float CameraViewWeight
        {
            get { return cameraViewWeight; }
            set { cameraViewWeight = value; }
        }

        public AnimationClip GripPose
        {
            get { return gripPose; }
            set { gripPose = value; }
        }

        public Transform GripIkTarget
        {
            get { return gripIkTarget; }
            set { gripIkTarget = value; }
        }

        public KTransform WeaponPose
        {
            get { return weaponPose; }
            set { weaponPose = value; }
        }

        public KTransform rightHandOffset
        {
            get { return rightHandOffsetValue; }
            set { rightHandOffsetValue = value; }
        }

        public KTransform leftHandOffset
        {
            get { return leftHandOffsetValue; }
            set { leftHandOffsetValue = value; }
        }

        public KTransform WeaponPoseThirdPerson
        {
            get { return weaponPoseThirdPerson; }
            set { weaponPoseThirdPerson = value; }
        }

        public Vector3 weaponPoseThirdPersonRestPosition
        {
            get { return weaponPoseThirdPersonRestPositionValue; }
            set { weaponPoseThirdPersonRestPositionValue = value; }
        }

        public Quaternion weaponPoseThirdPersonRestRotation
        {
            get { return weaponPoseThirdPersonRestRotationValue; }
            set { weaponPoseThirdPersonRestRotationValue = value; }
        }

        public Transform ScopeAimPoint
        {
            get { return scopeAimPoint; }
            set { scopeAimPoint = value; }
        }

        public bool IsAiming
        {
            get { return isAiming; }
            set { isAiming = value; }
        }

        public WeaponSway AimingSway
        {
            get { return aimingSway; }
            set { aimingSway = value; }
        }

        public WeaponSway MovementSway
        {
            get { return movementSway; }
            set { movementSway = value; }
        }

        public float Enable_HandR_IK
        {
            get { return enableHandRIk; }
            set { enableHandRIk = value; }
        }

        public Transform RightHandIkTarget
        {
            get { return rightHandIkTarget; }
            set { rightHandIkTarget = value; }
        }

        public float Enable_HandL_IK
        {
            get { return enableHandLIk; }
            set { enableHandLIk = value; }
        }

        public Transform LeftHandIkTarget
        {
            get { return leftHandIkTarget; }
            set { leftHandIkTarget = value; }
        }

        public float Interaction_HandL_IK
        {
            get { return interactionHandLIk; }
            set { interactionHandLIk = value; }
        }

        public Transform GetLeftHandTarget
        {
            get { return getLeftHandTarget; }
            set { getLeftHandTarget = value; }
        }

        public Transform GetLeftInteractionHandTarget
        {
            get { return getLeftInteractionHandTarget; }
            set { getLeftInteractionHandTarget = value; }
        }
    }
}