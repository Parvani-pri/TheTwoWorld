using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Progress
{
    public class ChapterBootstrap : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] string chapterId = "ch1_grave";
        [SerializeField] string chapterLabel = "第一章 · 祖坟";
        [SerializeField] string stageLabel = "调查祖坟";

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void Start()
        {
            if (gameProgress == null)
                return;

            gameProgress.SetChapter(chapterId, chapterLabel, stageLabel);
        }
    }
}
