using FishNet.Example;
using UnityEditor;
using UnityEngine;

namespace FishNet.Example.Editor
{
    [CustomEditor(typeof(RuntimeNetworkHud))]
    public class RuntimeNetworkHudEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Init Server"))
                    ((RuntimeNetworkHud)target).InitServer(true);
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Init Server can only be invoked while the game is running.", MessageType.Info);
        }
    }
}