using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwoWorlds.Progress
{
    public static class SceneFlow
    {
        public static void LoadYinLevel(int chapter)
        {
            if (!ChapterSceneCatalog.TryGetYinLevelSceneName(chapter, out var sceneName))
            {
                Debug.LogError($"[SceneFlow] No yin level scene configured for chapter {chapter}.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"[SceneFlow] Scene '{sceneName}' is not in Build Settings. Add it under File > Build Settings.");
                return;
            }

            Debug.Log($"[SceneFlow] Loading yin level scene '{sceneName}' for chapter {chapter}.");
            SceneManager.LoadScene(sceneName);
        }
    }
}
