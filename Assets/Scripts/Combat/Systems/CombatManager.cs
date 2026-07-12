using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public class CombatManager : MonoBehaviour
    {
        [SerializeField] bool autoStartOnLoad = true;

        CombatHealth playerHealth;
        int aliveEnemies;
        bool combatEnded;

        void Start()
        {
            if (autoStartOnLoad)
                BeginCombat();
        }

        public void BeginCombat()
        {
            if (combatEnded)
                return;

            playerHealth = null;
            aliveEnemies = 0;

            foreach (var health in FindObjectsByType<CombatHealth>(FindObjectsSortMode.None))
            {
                health.Died += OnActorDied;

                var actor = health.GetComponent<CombatActor>();
                if (actor == null)
                    continue;

                if (actor.Faction == CombatFaction.Player)
                    playerHealth = health;
                else
                    aliveEnemies++;
            }

            if (playerHealth == null)
                Debug.LogWarning("[CombatManager] No player CombatHealth found.");

            if (aliveEnemies == 0)
                Debug.LogWarning("[CombatManager] No enemy CombatHealth found.");

            GameEvents.RaiseCombatStarted();
        }

        void OnActorDied(CombatHealth health)
        {
            if (combatEnded)
                return;

            var actor = health.GetComponent<CombatActor>();
            if (actor == null)
                return;

            if (actor.Faction == CombatFaction.Player)
            {
                EndCombat(CombatResult.Defeat);
                return;
            }

            aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
            if (aliveEnemies == 0)
                EndCombat(CombatResult.Victory);
        }

        void EndCombat(CombatResult result)
        {
            if (combatEnded)
                return;

            combatEnded = true;
            GameEvents.RaiseCombatEnded(result);
        }
    }
}
