using TwoWorlds.Core;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    /// <summary>
    /// Hides Xiaomei in MainLobby after chapter 3 post-battle dialogue completes.
    /// </summary>
    public class XiaomeiLobbyPresence : MonoBehaviour
    {
        [SerializeField] GameProgress gameProgress;
        [SerializeField] int hideAfterChapter = 3;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        void OnEnable() => GameEvents.ScriptDialogueEnded += OnScriptDialogueEnded;

        void OnDisable() => GameEvents.ScriptDialogueEnded -= OnScriptDialogueEnded;

        void Start() => ApplyVisibility();

        void OnScriptDialogueEnded(DialogueEndInfo _) => ApplyVisibility();

        void ApplyVisibility()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            var postBattleId = ChapterProgressCatalog.GetDialogueId(hideAfterChapter, ChapterSegment.PostBattle);
            var shouldHide = gameProgress != null && gameProgress.HasDialogue(postBattleId);
            gameObject.SetActive(!shouldHide);
        }
    }

}
