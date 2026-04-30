using KINEMATION.CharacterAnimationSystem.Addons.FPS.Scripts.Modifiers;
using KINEMATION.CharacterAnimationSystem.Examples.Scripts;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Core;
using KINEMATION.ProceduralRecoilAnimationSystem.Runtime;
using KINEMATION.Shared.KAnimationCore.Runtime.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CAS_Demo.Scripts.FPS
{
    [AddComponentMenu(CasNames.Path_ComponentMenu + "FPS Example Controller")]
    public class FPSExampleController : CharacterExampleController
    {
        [Header("IK Motions")]
        [SerializeField] protected IkMotionSettings aimingMotion;
        [SerializeField] protected IkMotionSettings fireModeMotion;
        [SerializeField] protected IkMotionSettings movementMotion;

        protected RecoilAnimation _recoilAnimation;
        protected float _defaultMouseSensitivity;
        
        protected override void Start()
        {
            base.Start();
            _defaultMouseSensitivity = mouseSensitivity;
            _recoilAnimation = GetComponent<RecoilAnimation>();
        }

        protected override Vector2 GetDeltaLookInput()
        {
            Vector2 recoilDelta = Vector2.zero;

            if (_recoilAnimation != null)
            {
                _recoilAnimation.UpdateDeltaInput(_deltaLookInput);
                recoilDelta = _recoilAnimation.GetRecoilDelta();
            }
            
            return base.GetDeltaLookInput() + recoilDelta;
        }

#if UNITY_EDITOR
        public static string GetWeaponItemName => nameof(GetWeaponItem);
#endif
        
        public WeaponProp GetWeaponItem()
        {
            return GetActiveItem() as WeaponProp;
        }
        
        protected override void OnMovementChange(bool isMoving)
        {
            base.OnMovementChange(isMoving);
            if(_proceduralAnimation != null) _proceduralAnimation.UpdateAnimationModifier(movementMotion);
        }

        protected override void SyncProceduralData()
        {
            base.SyncProceduralData();

            if (_proceduralDataBehaviour == null || _proceduralDataBehaviour.proceduralData == null)
            {
                return;
            }

            var proceduralData = _proceduralDataBehaviour.proceduralData;
            var weapon = GetWeaponItem();

            proceduralData.IsAiming = _isAiming;

            if (weapon == null || weapon.weaponSettings == null)
            {
                proceduralData.GripPose = null;
                proceduralData.GripIkTarget = null;
                proceduralData.WeaponPose = KTransform.Identity;
                proceduralData.rightHandOffset = KTransform.Identity;
                proceduralData.leftHandOffset = KTransform.Identity;
                proceduralData.WeaponPoseThirdPerson = KTransform.Identity;
                proceduralData.weaponPoseThirdPersonRestPosition = Vector3.zero;
                proceduralData.weaponPoseThirdPersonRestRotation = Quaternion.identity;
                proceduralData.ScopeAimPoint = null;
                proceduralData.AimingSway = WeaponSway.shooterAimPreset;
                proceduralData.MovementSway = WeaponSway.shooterMovePreset;
                return;
            }

            var gripData = weapon.GetGripData();
            var scopeData = weapon.GetScopeData();
            var weaponSettings = weapon.weaponSettings;
            var aimingSway = weaponSettings.aimingSway;
            var movementSway = weaponSettings.movementSway;

            proceduralData.GripPose = gripData.pose;
            proceduralData.GripIkTarget = gripData.ikTarget;
            proceduralData.WeaponPose = weaponSettings.weaponPose;
            proceduralData.rightHandOffset = weaponSettings.rightHandOffset;
            proceduralData.leftHandOffset = weaponSettings.leftHandOffset;
            proceduralData.WeaponPoseThirdPerson = weaponSettings.weaponPoseThirdPerson;
            proceduralData.weaponPoseThirdPersonRestPosition = weaponSettings.weaponPoseThirdPersonRest.position;
            proceduralData.weaponPoseThirdPersonRestRotation = weaponSettings.weaponPoseThirdPersonRest.rotation;
            proceduralData.ScopeAimPoint = scopeData.aimPoint;
            proceduralData.AimingSway = aimingSway;
            proceduralData.MovementSway = movementSway;
        }

#if ENABLE_INPUT_SYSTEM
        public override void OnUseItem(InputValue value)
        {
            if (!isFirstPerson && !_isAiming) return;
            base.OnUseItem(value);
        }

        public virtual void OnChangeFiremode()
        {
            if(_proceduralAnimation != null) _proceduralAnimation.UpdateAnimationModifier(fireModeMotion);
            var weapon = GetWeaponItem();
            if (weapon == null) return;
            
            weapon.ChangeFireMode();
        }

        public virtual void OnAlternativeAim()
        {
            if (!_isAiming) return;
            
            var weapon = GetWeaponItem();
            if(weapon != null) weapon.OnAlternativeAim();
        }
        
        public virtual void OnDigitSelected(InputValue value)
        {
            var weapon = GetWeaponItem();
            if (weapon == null) return;
            
            int groupIndex = (int) value.Get<float>();
            
            weapon.CycleAttachments(groupIndex);
        }
        
        public override void OnAim(InputValue value)
        {
            base.OnAim(value);
            _proceduralAnimation.UpdateAnimationModifier(aimingMotion);
            mouseSensitivity = _isAiming ? _defaultMouseSensitivity * 0.7f : _defaultMouseSensitivity;

            if (!isFirstPerson && !_isAiming) GetActiveItem().StopUsingItem();
        }
        
        public virtual void OnReload()
        {
            GetWeaponItem().Reload();
        }
#endif
    }
}
