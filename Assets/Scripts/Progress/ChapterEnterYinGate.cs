using TwoWorlds.Core;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// Gameplay gate between 战前 and 入阴. Sets enter-yin unlock flag when the player interacts.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ChapterEnterYinGate : MonoBehaviour, IInteractable
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] int chapterNumber = 1;
        [SerializeField] string promptText = "入陰";
        [SerializeField] bool consumeAfterUnlock = true;

        bool consumed;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public bool CanInteract(GameObject interactor) => IsAvailable();

        public void Interact(GameObject interactor)
        {
            if (!IsAvailable())
                return;

            gameProgress.UnlockEnterYin(chapterNumber);

            if (consumeAfterUnlock)
                consumed = true;
        }

        public string GetPromptText() => promptText;

        bool IsAvailable()
        {
            if (gameProgress == null || consumed)
                return false;

            return gameProgress.HasFlag(ChapterProgressCatalog.GetPreBattleDoneFlag(chapterNumber)) &&
                   !gameProgress.HasFlag(ChapterProgressCatalog.GetEnterYinUnlockedFlag(chapterNumber));
        }
    }
}
