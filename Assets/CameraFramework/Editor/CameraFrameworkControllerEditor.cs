using UnityEditor;
using UnityEngine;

namespace CameraFramework.Editor
{
    [CustomEditor(typeof(CameraFrameworkController))]
    public sealed class CameraFrameworkControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var brain = serializedObject.FindProperty("brain");
            var profile = serializedObject.FindProperty("profile");
            var cameras = serializedObject.FindProperty("cameras");

            if (brain.objectReferenceValue == null || profile.objectReferenceValue == null || cameras.arraySize == 0)
                EditorGUILayout.HelpBox("Assign a Cinemachine Brain, effect profile and at least one camera slot.",
                    MessageType.Warning);

            if (!Application.isPlaying) return;
            var controller = (CameraFrameworkController)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play Mode Preview", EditorStyles.boldLabel);
            if (GUILayout.Button("Impact Shake")) controller.Shake(CameraEffectPreset.Impact);
            if (GUILayout.Button("FOV Pulse")) controller.PulseFov(-12f);
            if (GUILayout.Button("Recoil")) controller.Recoil(new Vector3(-4f, 0f, 0f));
            if (GUILayout.Button("Stop Effects")) controller.StopEffects();
        }
    }
}
