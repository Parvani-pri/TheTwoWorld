namespace TwoWorlds.Progress
{
    public static class ChapterSceneCatalog
    {
        public static bool TryGetYinLevelSceneName(int chapter, out string sceneName)
        {
            switch (chapter)
            {
                case 1:
                    sceneName = "Level1";
                    return true;
                case 2:
                    sceneName = "Level2";
                    return true;
                case 3:
                    sceneName = "Level3";
                    return true;
                default:
                    sceneName = null;
                    return false;
            }
        }
    }
}
