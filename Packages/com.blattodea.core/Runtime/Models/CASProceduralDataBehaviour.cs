using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Camera;
using UnityEngine;

namespace Blattodea.Core.Models
{
    public sealed class CASProceduralDataBehaviour : MonoBehaviour
    {
        [Tooltip("Inspector-visible CAS procedural model instance used for runtime binding.")]
        public CASProceduralData proceduralData = new();

        [Tooltip("Optional camera source. If not assigned, the first CharacterCamera in children is used.")]
        [SerializeField] private CharacterCamera characterCamera;

        private void Reset()
        {
            proceduralData ??= new CASProceduralData();
            characterCamera = GetComponentInChildren<CharacterCamera>(true);
        }

        private void OnValidate()
        {
            proceduralData ??= new CASProceduralData();
            if (characterCamera == null)
            {
                characterCamera = GetComponentInChildren<CharacterCamera>(true);
            }
        }

        private void LateUpdate()
        {
            proceduralData ??= new CASProceduralData();

            if (characterCamera == null)
            {
                characterCamera = GetComponentInChildren<CharacterCamera>(true);
            }

            if (characterCamera == null)
            {
                return;
            }

            proceduralData.CameraViewWeight = characterCamera.ViewWeight;
        }
    }
}