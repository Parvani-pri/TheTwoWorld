using System.Collections;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Combat
{
    public class CombatPlayerStatsUI : MonoBehaviour
    {
        [SerializeField] CombatHealth targetHealth;
        [SerializeField] Image playerHealthFill;
        [SerializeField] Image playerSprintFill;
        [SerializeField] Image playerFlightFill;
        [SerializeField] bool hideWhenFull;


        void Awake()
        {
        }

        void OnEnable()
        {
            GameEvents.ActorHealthChanged += OnActorHealthChanged;
            GameEvents.CombatStarted += OnCombatStarted;

            GameEvents.OnActorSprint += OnActorSprint;
            GameEvents.OnActorFlight += OnActorFlight;


            playerHealthFill.fillAmount = 1;
        }



        void OnDisable()
        {
            GameEvents.ActorHealthChanged -= OnActorHealthChanged;
            GameEvents.CombatStarted -= OnCombatStarted;

            GameEvents.OnActorSprint -= OnActorSprint;
            GameEvents.OnActorFlight -= OnActorFlight;
        }



        void Start() => RefreshFromTarget();

        void OnCombatStarted() => RefreshFromTarget();

        void OnActorHealthChanged(CombatHealth health, int current, int max)
        {
            if (health != targetHealth)
                return;

            Refresh(current, max);
        }
        private void OnActorSprint(float target, float max)
        {
            playerSprintFill.fillAmount = target / max;
        }
        private void OnActorFlight(float target, float max)
        {
            playerFlightFill.fillAmount = target / max;
        }



        void RefreshFromTarget()
        {
            if (targetHealth == null)
                return;

            Refresh(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }

        void Refresh(int current, int max)
        {
            var normalized = max > 0 ? (float)current / max : 0f;

            if (playerHealthFill != null)
            {
                StartCoroutine(HealthBarLerp(0.5f, normalized));
            }

            if (hideWhenFull && playerHealthFill != null && playerHealthFill.transform.parent != null)
                playerHealthFill.transform.parent.gameObject.SetActive(current < max);
        }

        IEnumerator HealthBarLerp(float duration, float target)
        {
            float timeLapsed = 0;
            float initialValue = playerHealthFill.fillAmount;
            while (timeLapsed < duration)
            {
                playerHealthFill.fillAmount = Mathf.Lerp(initialValue, target, timeLapsed / duration);
                timeLapsed += Time.deltaTime;
                yield return null;
            }
            playerHealthFill.fillAmount = target;
        }
    }
}
