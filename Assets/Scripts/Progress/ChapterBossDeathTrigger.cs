using TwoWorlds.Combat;
using TwoWorlds.Dialogue;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Progress
{
    /// <summary>
    /// When a chapter boss dies, marks combat complete and plays the post-battle dialogue.
    /// </summary>
    public class ChapterBossDeathTrigger : MonoBehaviour
    {
        [SerializeField] CombatHealth bossHealth;
        [SerializeField] ChapterDialogueTrigger postBattleDialogue;
        [SerializeField] GameProgress gameProgress;
        [SerializeField] int chapterNumber = 1;
        [SerializeField] string bossObjectName = "WangLiang_Scripted";

        bool handled;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            if (postBattleDialogue == null)
                postBattleDialogue = GetComponent<ChapterDialogueTrigger>();

            ResolveBossHealth();
        }

        void OnEnable()
        {
            ResolveBossHealth();
            SubscribeToBossDeath();
        }

        void OnDisable()
        {
            UnsubscribeFromBossDeath();
        }

        void Update()
        {
            if (handled || bossHealth != null)
                return;

            ResolveBossHealth();
            SubscribeToBossDeath();
        }

        void ResolveBossHealth()
        {
            if (bossHealth != null)
                return;

            bossHealth = FindBossHealth();
        }

        void SubscribeToBossDeath()
        {
            if (bossHealth == null)
                return;

            bossHealth.Died -= OnBossDied;
            bossHealth.Died += OnBossDied;
        }

        void UnsubscribeFromBossDeath()
        {
            if (bossHealth != null)
                bossHealth.Died -= OnBossDied;
        }

        void OnBossDied(CombatHealth _)
        {
            if (handled)
                return;

            handled = true;

            if (gameProgress != null)
                gameProgress.MarkCombatDone(chapterNumber);

            if (postBattleDialogue == null)
            {
                Debug.LogWarning("[ChapterBossDeathTrigger] Post-battle dialogue trigger is missing.");
                return;
            }

            var player = FindFirstObjectByType<PlayerInventory>();
            if (player == null)
            {
                Debug.LogWarning("[ChapterBossDeathTrigger] PlayerInventory not found.");
                return;
            }

            postBattleDialogue.TriggerDialogue(player.gameObject);
        }

        CombatHealth FindBossHealth()
        {
            foreach (var health in FindObjectsByType<CombatHealth>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (health.Faction != CombatFaction.Enemy)
                    continue;

                if (string.IsNullOrWhiteSpace(bossObjectName) ||
                    health.gameObject.name.Contains(bossObjectName))
                    return health;
            }

            return null;
        }
    }

}
