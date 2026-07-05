using UnityEditor;
using UnityEngine;

namespace XuFu.MaskSystem.Editor
{
    [CustomEditor(typeof(MaskAnimationProfile))]
    public class MaskAnimationProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var profile = (MaskAnimationProfile)target;

            EditorGUILayout.HelpBox(
                "Animation Name must match the Animator State Relay name exactly. Example: idle, walk, run, jump, attack1, attack2.",
                MessageType.Info);

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Quick Set Animation Name", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("idle")) SetName(profile, "idle");
                if (GUILayout.Button("walk")) SetName(profile, "walk");
                if (GUILayout.Button("run")) SetName(profile, "run");
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("jump")) SetName(profile, "jump");
                if (GUILayout.Button("attack1")) SetName(profile, "attack1");
                if (GUILayout.Button("attack2")) SetName(profile, "attack2");
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Set From Asset Name"))
            {
                string assetName = target.name.ToLowerInvariant();
                if (assetName.Contains("attack2")) SetName(profile, "attack2");
                else if (assetName.Contains("attack1")) SetName(profile, "attack1");
                else if (assetName.Contains("attack")) SetName(profile, "attack1");
                else if (assetName.Contains("jump")) SetName(profile, "jump");
                else if (assetName.Contains("run")) SetName(profile, "run");
                else if (assetName.Contains("walk")) SetName(profile, "walk");
                else if (assetName.Contains("idle")) SetName(profile, "idle");
                else Debug.LogWarning("Cannot infer animation name from asset name: " + target.name);
            }
        }

        private static void SetName(MaskAnimationProfile profile, string value)
        {
            Undo.RecordObject(profile, "Set Mask Animation Name");
            profile.animationName = value;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }
    }
}
