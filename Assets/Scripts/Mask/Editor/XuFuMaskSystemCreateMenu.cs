using UnityEditor;
using UnityEngine;

namespace XuFu.MaskSystem.Editor
{
    public static class XuFuMaskSystemCreateMenu
    {
        [MenuItem("Assets/Create/XuFu/Mask System/Profile/Idle", priority = 1)]
        public static void CreateIdleProfile() => CreateProfile("idle", "IdleProfile.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Profile/Walk", priority = 2)]
        public static void CreateWalkProfile() => CreateProfile("walk", "WalkProfile.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Profile/Run", priority = 3)]
        public static void CreateRunProfile() => CreateProfile("run", "RunProfile.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Profile/Jump", priority = 4)]
        public static void CreateJumpProfile() => CreateProfile("jump", "JumpProfile.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Profile/Attack1", priority = 5)]
        public static void CreateAttack1Profile() => CreateProfile("attack1", "Attack1Profile.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Profile/Attack2", priority = 6)]
        public static void CreateAttack2Profile() => CreateProfile("attack2", "Attack2Profile.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Mask Database", priority = 20)]
        public static void CreateDatabase() => CreateAsset<MaskDatabase>("MaskDatabase.asset");

        [MenuItem("Assets/Create/XuFu/Mask System/Mask Item", priority = 21)]
        public static void CreateMaskItem() => CreateAsset<MaskItem>("MaskItem.asset");

        private static void CreateProfile(string animationName, string fileName)
        {
            var asset = ScriptableObject.CreateInstance<MaskAnimationProfile>();
            asset.animationName = animationName;
            EditorUtility.SetDirty(asset);
            CreateAsset(asset, fileName);
        }

        private static void CreateAsset<T>(string fileName) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            EditorUtility.SetDirty(asset);
            CreateAsset(asset, fileName);
        }

        private static void CreateAsset(ScriptableObject asset, string fileName)
        {
            string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(folder)) folder = "Assets";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                folder = System.IO.Path.GetDirectoryName(folder);
                if (!string.IsNullOrEmpty(folder)) folder = folder.Replace("\\", "/");
            }
            if (string.IsNullOrEmpty(folder)) folder = "Assets";

            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + fileName);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
