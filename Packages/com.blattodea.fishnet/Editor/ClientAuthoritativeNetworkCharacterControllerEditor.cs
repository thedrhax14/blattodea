using System.Reflection;
using Blattodea.FishNet.Controllers;
using UnityEditor;
using UnityEngine;

namespace Blattodea.FishNet.Editor
{
    [CustomEditor(typeof(ClientAuthoritativeNetworkCharacterController))]
    public class ClientAuthoritativeNetworkCharacterControllerEditor : UnityEditor.Editor
    {
        private static readonly FieldInfo VelocityField = typeof(ClientAuthoritativeNetworkCharacterController)
            .GetField("_velocity", BindingFlags.Instance | BindingFlags.NonPublic);

        private static GUIStyle _labelStyle;

        private static GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle != null)
                {
                    return _labelStyle;
                }

                _labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 12
                };
                _labelStyle.normal.textColor = new Color(0.85f, 0.95f, 1f);
                return _labelStyle;
            }
        }

        private void OnSceneGUI()
        {
            if (VelocityField == null)
            {
                return;
            }

            var controller = (ClientAuthoritativeNetworkCharacterController)target;
            Vector3 velocity = (Vector3)VelocityField.GetValue(controller);
            Vector3 labelPosition = controller.transform.position + Vector3.up * 2.2f;

            Handles.Label(
                labelPosition,
                $"X Velocity: {velocity.x:F2}\nZ Velocity: {velocity.z:F2}",
                LabelStyle);
        }
    }
}