using TMPro;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public class CombatResultUI : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] TMP_Text resultText;
        [SerializeField] string victoryMessage = "Victory!";
        [SerializeField] string defeatMessage = "Defeat...";

        void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        void OnEnable() => GameEvents.CombatEnded += OnCombatEnded;

        void OnDisable() => GameEvents.CombatEnded -= OnCombatEnded;

        void OnCombatEnded(CombatResult result)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (resultText != null)
            {
                resultText.text = result == CombatResult.Victory
                    ? victoryMessage
                    : defeatMessage;
            }
        }
    }
}
