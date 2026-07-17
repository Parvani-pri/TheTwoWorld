using System.Collections;
using TwoWorlds.Dialogue;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// Automatically plays a ChapterDialogueTrigger once when the scene starts.
    /// </summary>
    public class ChapterDialogueAutoTrigger : MonoBehaviour
    {
        [SerializeField] ChapterDialogueTrigger dialogueTrigger;
        [SerializeField] float startDelaySeconds;

        IEnumerator Start()
        {
            if (dialogueTrigger == null)
                dialogueTrigger = GetComponent<ChapterDialogueTrigger>();

            if (startDelaySeconds > 0f)
                yield return new WaitForSeconds(startDelaySeconds);

            var player = FindFirstObjectByType<PlayerInventory>();
            if (player == null)
            {
                Debug.LogWarning("[ChapterDialogueAutoTrigger] PlayerInventory not found.");
                yield break;
            }

            if (dialogueTrigger == null)
            {
                Debug.LogWarning("[ChapterDialogueAutoTrigger] ChapterDialogueTrigger is missing.");
                yield break;
            }

            dialogueTrigger.TriggerDialogue(player.gameObject);
        }
    }
}
