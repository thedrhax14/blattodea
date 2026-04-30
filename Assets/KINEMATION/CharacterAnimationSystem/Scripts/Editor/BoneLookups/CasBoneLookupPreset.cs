// Copyright (c) 2026 KINEMATION.
// All rights reserved.

namespace KINEMATION.CharacterAnimationSystem.Scripts.Editor.BoneLookups
{
    public class CasBoneLookupPreset
    {
        protected static readonly string[] RightPrefixes = new[] {"right", "r_", "right_", "r."};
        protected static readonly string[] RightPostfixes = new[] {"right", "_r", "_right", ".r"};
        
        protected static readonly string[] LeftPrefixes = new[] {"left", "l_", "left_", "l."};
        protected static readonly string[] LeftPostfixes = new[] {"left", "_l", "_left", ".l"};
        
        protected static readonly string[] GeneralAvoidLookups = new[] {"ik"};

        public virtual string GetName()
        {
            return "Default";
        }

        public virtual CasQuickSkeletonRecognizer GetQuickSkeletonRecognizer()
        {
            return default;
        }
        
        // Lookups for bone chains.
        public virtual CasBoneNameLookups GetLowerBodyLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[]
                {
                    "skeleton", "armature", "root", "pelvis", "hip", "leg", "thigh", "calf", "foot", "ball", "toe"
                },
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetSpineLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"spine"},
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetHeadLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"neck", "head"},
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetRightArmLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"arm", "clavicle", "shoulder", "hand"},
                prefixes = RightPrefixes,
                postfixes = RightPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetLeftArmLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"arm", "clavicle", "shoulder", "hand"},
                prefixes = LeftPrefixes,
                postfixes = LeftPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetFingersLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"finger", "index", "thumb", "pinky", "ring", "middle"},
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }
        
        public virtual CasBoneNameLookups GetLeftHandFingersLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"finger", "index", "thumb", "pinky", "ring", "middle"},
                prefixes = LeftPrefixes,
                postfixes = LeftPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }
        
        public virtual CasBoneNameLookups GetRightHandFingersLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"finger", "index", "thumb", "pinky", "ring", "middle"},
                prefixes = RightPrefixes,
                postfixes = RightPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }
        
        // Lookups for individual bones.
        public virtual CasBoneNameLookups GetRightHandLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"hand", "palm", "wrist"},
                prefixes = RightPrefixes,
                postfixes = RightPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetLeftHandLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"hand", "palm", "wrist"},
                prefixes = LeftPrefixes,
                postfixes = LeftPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetRightFootLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"foot", "ankle"},
                prefixes = RightPrefixes,
                postfixes = RightPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetLeftFootLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"foot", "ankle"},
                prefixes = LeftPrefixes,
                postfixes = LeftPostfixes,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public virtual CasBoneNameLookups GetPelvisLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"pelvis", "hip"},
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }
    }
    
    public class Mixamo_CasBoneLookupPreset : CasBoneLookupPreset
    {
        private static string MixamoRightPrefix = "mixamorig:right";
        private static string MixamoLeftPrefix = "mixamorig:left";
        private static string[] MixamoArmAvoidQueries = new[] {"ik", "index", "middle", "ring", "pinky", "thumb"};

        public override CasQuickSkeletonRecognizer GetQuickSkeletonRecognizer()
        {
            return new CasQuickSkeletonRecognizer
            {
                queries = new[] {"mixamorig:"},
                minMatches = 1
            };
        }
        
        public override CasBoneNameLookups GetRightArmLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"arm", "clavicle", "shoulder", "hand"},
                prefixes = new [] {MixamoRightPrefix},
                postfixes = null,
                avoidQueries = MixamoArmAvoidQueries
            };
        }

        public override CasBoneNameLookups GetLeftArmLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"arm", "clavicle", "shoulder", "hand"},
                prefixes = new [] {MixamoLeftPrefix},
                postfixes = null,
                avoidQueries = MixamoArmAvoidQueries
            };
        }

        public override CasBoneNameLookups GetRightHandLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {MixamoRightPrefix + "hand"},
                prefixes = null,
                postfixes = null,
                avoidQueries = MixamoArmAvoidQueries
            };
        }

        public override CasBoneNameLookups GetLeftHandLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {MixamoLeftPrefix + "hand"},
                prefixes = null,
                postfixes = null,
                avoidQueries = MixamoArmAvoidQueries
            };
        }

        public override CasBoneNameLookups GetRightFootLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {MixamoRightPrefix + "foot"},
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetLeftFootLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {MixamoLeftPrefix + "foot"},
                prefixes = null,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetSpineLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"spine"},
                prefixes = new [] {"mixamorig:"},
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override string GetName()
        {
            return "Mixamo";
        }
    }
    
    public class Synty_CasBoneLookupPreset : CasBoneLookupPreset
    {
        public override CasQuickSkeletonRecognizer GetQuickSkeletonRecognizer()
        {
            return new CasQuickSkeletonRecognizer
            {
                queries = new[] {"upperleg_r", "ankle_r", "ball_r"},
                minMatches = 1
            };
        }

        public override string GetName()
        {
            return "Synty";
        }
    }
    
    public class CharacterCreator_CasBoneLookupPreset : CasBoneLookupPreset
    {
        private static string[] CcPrefix = new[] {"cc_base_"};
        private static string[] CcPrefixRight = new[] {"cc_base_r_"};
        private static string[] CcPrefixLeft = new[] {"cc_base_l_"};

        public override CasQuickSkeletonRecognizer GetQuickSkeletonRecognizer()
        {
            return new CasQuickSkeletonRecognizer
            {
                queries = new[] {"cc_base_"},
                minMatches = 1
            };
        }
        
        public override string GetName()
        {
            return "Character Creator (CC4, CC5)";
        }
        
        public override CasBoneNameLookups GetLowerBodyLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[]
                {
                    "boneroot", "pelvis", "hip", "leg", "thigh", "calf", "foot", "ball", "toe"
                },
                prefixes = CcPrefix,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetSpineLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"waist"},
                prefixes = CcPrefix,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetRightArmLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"clavicle", "arm", "hand"},
                prefixes = CcPrefixRight,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetLeftArmLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"clavicle"},
                prefixes = CcPrefixLeft,
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }
        
        public override CasBoneNameLookups GetFingersLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"finger", "index", "thumb", "pinky", "ring", "middle", "mid1"},
                prefixes = null,
                postfixes = null,
                avoidQueries = new [] {"ik", "toe"}
            };
        }

        public override CasBoneNameLookups GetLeftHandFingersLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"finger", "index", "thumb", "pinky", "ring", "middle", "mid1"},
                prefixes = CcPrefixLeft,
                postfixes = null,
                avoidQueries = new [] {"ik", "toe"}
            };
        }

        public override CasBoneNameLookups GetRightHandFingersLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"finger", "index", "thumb", "pinky", "ring", "middle", "mid1"},
                prefixes = CcPrefixRight,
                postfixes = null,
                avoidQueries = new [] {"ik", "toe"}
            };
        }
    }
    
    public class UnrealManny_CasBoneLookupPreset : CasBoneLookupPreset
    {
        public override CasQuickSkeletonRecognizer GetQuickSkeletonRecognizer()
        {
            return new CasQuickSkeletonRecognizer
            {
                queries = new[] {"spine_01", "upperarm_r", "upperarm_l", "hand_r", "hand_l", "foot_r", "foot_l"},
                minMatches = 3
            };
        }

        public override CasBoneNameLookups GetLowerBodyLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[]
                {
                    "skeleton", "armature", "root", "pelvis", "hip", "leg", "thigh", "calf", "foot", "ball", "toe"
                },
                prefixes = null,
                postfixes = null,
                avoidQueries = new[] {"ik", "arm"}
            };
        }

        public override CasBoneNameLookups GetRightHandLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"hand_r"},
                prefixes = new[] {"hand"},
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetLeftHandLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"hand_l"},
                prefixes = new[] {"hand"},
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetRightFootLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"foot_r"},
                prefixes = new[] {"foot"},
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override CasBoneNameLookups GetLeftFootLookups()
        {
            return new CasBoneNameLookups()
            {
                queries = new[] {"foot_l"},
                prefixes = new[] {"foot"},
                postfixes = null,
                avoidQueries = GeneralAvoidLookups
            };
        }

        public override string GetName()
        {
            return "Unreal Mannequin";
        }
    }
}
