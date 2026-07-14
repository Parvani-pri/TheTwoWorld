using System.IO;
using UnityEngine;

namespace TwoWorlds.AI
{
    [System.Serializable]
    class AISecretsFile
    {
        public string apiKey;
    }

    static class AISecretsLoader
    {
        public static string TryLoadApiKey(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            if (!File.Exists(path))
                return null;

            try
            {
                var json = File.ReadAllText(path);
                var secrets = JsonUtility.FromJson<AISecretsFile>(json);
                return string.IsNullOrWhiteSpace(secrets?.apiKey) ? null : secrets.apiKey.Trim();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AISecretsLoader] Failed to read secrets file: {ex.Message}");
                return null;
            }
        }
    }
}
