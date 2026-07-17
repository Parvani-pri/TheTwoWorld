using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// Gameplay gate between 战后 and 尾声. Sets exit-battle unlock flag when the player returns from combat.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ChapterExitBattleGate : MonoBehaviour, IInteractable
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] int chapterNumber = 1;
        [SerializeField] string promptText = "离开";
        [SerializeField] bool autoUnlockOnStart;
        [SerializeField] bool consumeAfterUnlock = true;

        bool consumed;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void Start()
        {
            if (!autoUnlockOnStart || gameProgress == null)
                return;

            var postBattleId = ChapterProgressCatalog.GetDialogueId(chapterNumber, ChapterSegment.PostBattle);
            if (gameProgress.HasDialogue(postBattleId))
                gameProgress.UnlockExitBattle(chapterNumber);
        }

        public bool CanInteract(GameObject interactor) => IsAvailable();

        public void Interact(GameObject interactor)
        {
            if (!IsAvailable())
                return;

            gameProgress.UnlockExitBattle(chapterNumber);

            if (consumeAfterUnlock)
                consumed = true;
        }

        public string GetPromptText() => promptText;

        bool IsAvailable()
        {
            if (gameProgress == null || consumed)
                return false;

            var postBattleId = ChapterProgressCatalog.GetDialogueId(chapterNumber, ChapterSegment.PostBattle);
            return gameProgress.HasDialogue(postBattleId) &&
                   !gameProgress.HasFlag(ChapterProgressCatalog.GetExitBattleUnlockedFlag(chapterNumber));
        }
    }
}
