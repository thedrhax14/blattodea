// Copyright (c) 2026 KINEMATION.
// All rights reserved.

using System;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Core;
using KINEMATION.Shared.KAnimationCore.Runtime.Rig;
using KINEMATION.Shared.ScriptableWidget.Runtime;
using UnityEngine;
using UnityEngine.Animations;

namespace KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Modifiers.IK
{
    [Serializable]
    public struct LimbIKSettings
    {
        public KRigElement tip;
        public KRigElement target;
        public KRigElement poleTarget;
        [Tooltip("Fallback pole direction in the mid joint local space when no pole target is bound.")]
        public Vector3 poleVector;
        
        [Range(0f, 1f)] public float weight;
        [Range(0f, 1f)] public float poleWeight;

        [NonSerialized] public TransformStreamHandle tipHandle;
        [NonSerialized] public TransformStreamHandle midHandle;
        [NonSerialized] public TransformStreamHandle rootHandle;
        [NonSerialized] public TransformStreamHandle targetHandle;
        [NonSerialized] public TransformStreamHandle poleHandle;
        [NonSerialized] public Vector3 localJointForward;
        [NonSerialized] public Vector3 localJointUp;
        
        public void Initialize(ModifierJobData jobData)
        {
            tipHandle = midHandle = rootHandle = targetHandle = poleHandle = new TransformStreamHandle();
            localJointForward = Vector3.forward;
            localJointUp = Vector3.up;

            if (poleVector.sqrMagnitude <= Mathf.Epsilon)
            {
                poleVector = Vector3.forward;
            }

            Transform tipTransform = jobData.skeleton.GetBoneTransform(tip.name);
            
            if (tipTransform == null)
            {
                Debug.LogWarning($"Failed to initialize IK for {tip.name}: no tip found!");
                return;
            }
            
            Transform midTransform = tipTransform.parent;
            if (midTransform == null)
            {
                Debug.LogWarning($"Failed to initialize IK for {tip.name}: no mid (elbow, knee) found!");
                return;
            }
            
            Transform rootTransform = midTransform.parent;
            if (rootTransform == null)
            {
                Debug.LogWarning($"Failed to initialize IK for {tip.name}: no root (arm, leg) found!");
                return;
            }
            
            tipHandle = AnimationModifierUtility.GetHandle(jobData, tip);
            midHandle = AnimationModifierUtility.GetHandle(jobData, 
                new KRigElement(-1, midTransform.name));
            rootHandle = AnimationModifierUtility.GetHandle(jobData, 
                new KRigElement(-1, rootTransform.name));
            
            targetHandle = AnimationModifierUtility.GetHandle(jobData, target);
            poleHandle = AnimationModifierUtility.GetHandle(jobData, poleTarget);

            Transform characterRoot = jobData.animator.transform;
            localJointForward = AnimationModifierUtility.DetectClosestLocalAxis(midTransform.rotation,
                characterRoot.forward);
            localJointUp = AnimationModifierUtility.DetectClosestLocalAxis(midTransform.rotation,
                characterRoot.up);
        }
    }
    
    [ScriptableComponentGroup("IK", "Full Body IK")]
    public class FullBodyIkSettings : AnimationModifierSettings
    {
        public LimbIKSettings rightHandIk = new LimbIKSettings()
        {
            tip = new KRigElement(-1),
            target = new KRigElement(-1, CasNames.Bone_IkHandRight),
            poleTarget = new KRigElement(-1),
            poleVector = Vector3.forward,
            weight = 1f,
            poleWeight = 1f,
        };
        
        public LimbIKSettings leftHandIk = new LimbIKSettings()
        {
            tip = new KRigElement(-1),
            target = new KRigElement(-1, CasNames.Bone_IkHandLeft),
            poleTarget = new KRigElement(-1),
            poleVector = Vector3.forward,
            weight = 1f,
            poleWeight = 1f,
        };
        
        public LimbIKSettings rightFootIk = new LimbIKSettings()
        {
            tip = new KRigElement(-1),
            target = new KRigElement(-1, CasNames.Bone_IkFootRight),
            poleTarget = new KRigElement(-1),
            poleVector = Vector3.forward,
            weight = 1f,
            poleWeight = 1f,
        };
        
        public LimbIKSettings leftFootIk = new LimbIKSettings()
        {
            tip = new KRigElement(-1),
            target = new KRigElement(-1, CasNames.Bone_IkFootLeft),
            poleTarget = new KRigElement(-1),
            poleVector = Vector3.forward,
            weight = 1f,
            poleWeight = 1f,
        };

        public override IAnimationModifierJob CreateAnimationJob()
        {
            return new FullBodyIkJob();
        }
    }
}
