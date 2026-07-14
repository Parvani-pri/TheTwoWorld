using TMPro;
using TwoWorlds.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.AI
{
    public class AITestHarness : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] AIService aiService;
        [SerializeField] AIContextBuilder contextBuilder;
        [SerializeField] PlayerInventory testInventory;

        [Header("Optional UI")]
        [SerializeField] TMP_Text outputText;
        [SerializeField] Button testIdentityButton;
        [SerializeField] Button testInventoryButton;
        [SerializeField] Button testItemButton;

        [Header("Test Items")]
        [SerializeField] ItemData ancientKey;
        [SerializeField] ItemData soulNail;

        [Header("Editor Test Setup")]
        [SerializeField] bool seedTestItemsInEditor = false;

        const string SimpleSystemPrompt =
            "你是《两界》中的叙事向导。用简体中文简短回答，每次 1-2 句话。";

        void Awake()
        {
            if (aiService == null)
                aiService = GetComponent<AIService>();

            if (contextBuilder == null)
                contextBuilder = GetComponent<AIContextBuilder>();
        }

        void Start()
        {
            if (testInventory == null)
                testInventory = FindFirstObjectByType<PlayerInventory>();

            if (testIdentityButton != null)
                testIdentityButton.onClick.AddListener(RunIdentityTest);

            if (testInventoryButton != null)
                testInventoryButton.onClick.AddListener(RunInventoryTest);

            if (testItemButton != null)
                testItemButton.onClick.AddListener(RunItemInterpretTest);

#if UNITY_EDITOR
            if (seedTestItemsInEditor)
                SeedTestItems();
#endif
        }

        void OnDestroy()
        {
            if (testIdentityButton != null)
                testIdentityButton.onClick.RemoveListener(RunIdentityTest);

            if (testInventoryButton != null)
                testInventoryButton.onClick.RemoveListener(RunInventoryTest);

            if (testItemButton != null)
                testItemButton.onClick.RemoveListener(RunItemInterpretTest);
        }

        void SeedTestItems()
        {
            if (testInventory == null)
                return;

            if (ancientKey != null && testInventory.GetItemCount(ancientKey) == 0)
                testInventory.AddItem(ancientKey, 1);

            if (soulNail != null && testInventory.GetItemCount(soulNail) == 0)
                testInventory.AddItem(soulNail, 1);
        }

        [ContextMenu("Run Identity Test")]
        public void RunIdentityTest()
        {
            SetStatus("Test 1 请求中：你是谁？");
            aiService.Ask(
                SimpleSystemPrompt,
                "你是谁？",
                response => SetStatus("Test 1 成功：\n" + response),
                error => SetStatus("Test 1 失败：\n" + error));
        }

        [ContextMenu("Run Inventory Test")]
        public void RunInventoryTest()
        {
            SetStatus("Test 2 请求中：我背包里有什么？");
            aiService.AskWithInventoryContext(
                testInventory,
                null,
                "我背包里有什么？",
                response => SetStatus("Test 2 成功：\n" + response),
                error => SetStatus("Test 2 失败：\n" + error));
        }

        [ContextMenu("Run Item Interpret Test")]
        public void RunItemInterpretTest()
        {
            var item = soulNail != null ? soulNail : ancientKey;
            if (item == null)
            {
                SetStatus("Test 3 失败：请在 Inspector 指定 SoulNail 或 AncientKey。");
                return;
            }

            SetStatus($"Test 3 请求中：解读 {item.DisplayName}");
            aiService.AskItemInterpretation(
                item,
                response => SetStatus("Test 3 成功：\n" + response),
                error => SetStatus("Test 3 失败：\n" + error));
        }

        void SetStatus(string message)
        {
            Debug.Log("[AITestHarness] " + message);

            if (outputText != null)
                outputText.text = message;
        }
    }
}
