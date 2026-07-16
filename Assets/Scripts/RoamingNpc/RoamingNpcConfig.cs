using UnityEngine;

namespace TwoWorlds.RoamingNpc
{
    [CreateAssetMenu(fileName = "RoamingNpcConfig", menuName = "Two Worlds/Roaming NPC Config")]
    public class RoamingNpcConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string displayName = "小妹";
        [SerializeField] Sprite portrait;
        [TextArea(3, 6)]
        [SerializeField] string interruptPersona =
            "你是「小妹」，玩家的助手，性格活泼、调皮，偶尔跑来打断玩家。";

        [Header("Roaming")]
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float wanderRadius = 10f;
        [SerializeField] float pickTargetIntervalMin = 3f;
        [SerializeField] float pickTargetIntervalMax = 7f;
        [SerializeField] float arriveThreshold = 0.25f;

        [Header("Interrupt")]
        [SerializeField] float interruptCooldownSeconds = 90f;
        [SerializeField] float senseRadius = 12f;
        [SerializeField] float approachStopDistance = 1.2f;
        [SerializeField] float chaseCancelDistance = 18f;
        [SerializeField] float failedChaseCooldownSeconds = 10f;
        [TextArea(2, 4)]
        [SerializeField] string postApproachOpeningMessage = "嘿，等等我呀！";

        [Header("Presentation")]
        [SerializeField] float interruptTypewriterCps = 45f;
        [SerializeField] string[] fallbackInterruptLines =
        {
            "喂喂，走那么快干嘛，等等我呀！",
            "你背包里装的东西可真有意思。",
            "别瞎晃了，快看看前面！"
        };

        public string DisplayName => displayName;
        public Sprite Portrait => portrait;
        public string InterruptPersona => interruptPersona;
        public float MoveSpeed => moveSpeed;
        public float WanderRadius => wanderRadius;
        public float PickTargetIntervalMin => pickTargetIntervalMin;
        public float PickTargetIntervalMax => pickTargetIntervalMax;
        public float ArriveThreshold => arriveThreshold;
        public float InterruptCooldownSeconds => interruptCooldownSeconds;
        public float SenseRadius => senseRadius;
        public float ApproachStopDistance => approachStopDistance;
        public float ChaseCancelDistance => chaseCancelDistance;
        public float FailedChaseCooldownSeconds => failedChaseCooldownSeconds;
        public string PostApproachOpeningMessage => postApproachOpeningMessage;
        public float InterruptTypewriterCps => interruptTypewriterCps;
        public string[] FallbackInterruptLines => fallbackInterruptLines;

        public string GetRandomFallbackLine()
        {
            if (fallbackInterruptLines == null || fallbackInterruptLines.Length == 0)
                return "嘿，等等我！";

            return fallbackInterruptLines[Random.Range(0, fallbackInterruptLines.Length)];
        }
    }
}
