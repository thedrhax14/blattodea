// Copyright (c) 2026 KINEMATION.
// All rights reserved.

using System.IO;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Playables;
using KINEMATION.Shared.KAnimationCore.Editor;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.CharacterAnimationSystem.Scripts.Editor.Core
{
    public class AnimationAssetCreator
    {
        [MenuItem("Assets/Create/KINEMATION/CAS/Animation Asset", true, 0)]
        private static bool ValidateCreateAnimationAsset()
        {
            return true;
        }

        [MenuItem("Assets/Create/KINEMATION/CAS/Animation Asset", false, 0)]
        private static void CreateAnimationAsset()
        {
            AnimationClip clip = KEditorUtility.GetAnimationClipFromSelection();
            AnimationAsset animAsset = ScriptableObject.CreateInstance<AnimationAsset>();

            string assetPath = KEditorUtility.GetProjectActiveFolder();
            string assetName = "A_NewAnimation.asset";
            
            if (clip != null)
            {
                animAsset.clip = clip;
                assetName = $"A_{clip.name}.asset";
                assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                assetPath = Path.GetDirectoryName(assetPath);
            }
            
            KEditorUtility.SaveAsset(animAsset, assetPath, assetName);
        }
    }
}