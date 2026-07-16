using System.Collections.Generic;
using System.Text;
using TwoWorlds.AI;
using TwoWorlds.Inventory;
using TwoWorlds.Progress;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwoWorlds.AI
{
    public class AIContextBuilder : MonoBehaviour
    {
        [Header("NPC Persona Defaults")]
        [SerializeField] string defaultNpcName = "小妹";
        [TextArea(3, 6)]
        [SerializeField] string defaultNpcPersona =
            "你是「小妹」，玩家的助手，性格活泼、调皮，熟悉地府与傩面等传说。说话简短口语化，站在玩家这边。";

        [Header("Services")]
        [SerializeField] GameProgress gameProgress;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public string BuildInventorySummary(PlayerInventory inventory)
        {
            if (inventory == null)
                return "玩家背包信息不可用。";

            var itemTotals = new Dictionary<ItemData, int>();
            foreach (var slot in inventory.Slots)
            {
                if (slot.IsEmpty)
                    continue;

                if (itemTotals.ContainsKey(slot.item))
                    itemTotals[slot.item] += slot.quantity;
                else
                    itemTotals[slot.item] = slot.quantity;
            }

            if (itemTotals.Count == 0)
                return "玩家背包为空。";

            var builder = new StringBuilder();
            builder.Append("玩家当前背包（共 ").Append(itemTotals.Count).Append(" 种物品）：");

            foreach (var pair in itemTotals)
            {
                var item = pair.Key;
                builder.Append("\n- ")
                    .Append(item.DisplayName)
                    .Append(" (")
                    .Append(item.ItemId)
                    .Append(")：")
                    .Append(item.Description)
                    .Append(" ×")
                    .Append(pair.Value);
            }

            return builder.ToString();
        }

        public string BuildSceneContext() =>
            SceneManager.GetActiveScene().name;

        public string BuildProgressSummary(AIContextLevel level)
        {
            if (level == AIContextLevel.None)
                return string.Empty;

            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();

            return gameProgress != null
                ? gameProgress.BuildProgressSummary(level)
                : "【游戏进度】暂无记录。";
        }

        public string BuildNpcSystemPrompt(
            string npcName,
            string persona,
            PlayerInventory inventory,
            AIContextLevel progressLevel = AIContextLevel.Full)
        {
            var resolvedName = string.IsNullOrWhiteSpace(npcName) ? defaultNpcName : npcName.Trim();
            var resolvedPersona = string.IsNullOrWhiteSpace(persona) ? defaultNpcPersona : persona.Trim();
            var inventorySummary = BuildInventorySummary(inventory);
            var sceneName = BuildSceneContext();
            var progressSummary = BuildProgressSummary(progressLevel);

            return
                $"你是《两界》(TheTwoWorld) 中的「{resolvedName}」。\n" +
                $"{resolvedPersona}\n\n" +
                "【当前状态】\n" +
                $"场景：{sceneName}\n" +
                $"{progressSummary}\n" +
                $"{inventorySummary}\n\n" +
                "【回复规则】\n" +
                "1. 使用简体中文，每次 1-3 句话，口语化。\n" +
                "2. 可以引用玩家背包中的物品名进行回答。\n" +
                "3. 不要编造玩家没有的物品。\n" +
                "4. 只能引用【游戏进度】中已出现的内容，不要剧透未解锁剧情。\n" +
                "5. 不确定的问题回答「我不太清楚」。\n" +
                "6. 未在进度中出现的内容，请用角色口吻回避。";
        }

        public string BuildInterruptPrompt(PlayerInventory inventory, string personaOverride = null)
        {
            var persona = string.IsNullOrWhiteSpace(personaOverride) ? defaultNpcPersona : personaOverride.Trim();
            var inventorySummary = BuildInventorySummary(inventory);
            var sceneName = BuildSceneContext();
            var progressSummary = BuildProgressSummary(AIContextLevel.Compact);

            return
                "你是《两界》中的「小妹」，玩家的助手，性格活泼、调皮。\n" +
                $"{persona}\n" +
                "你刚刚跑到玩家面前，想随口打断他说一句话。\n\n" +
                "【当前状态】\n" +
                $"场景：{sceneName}\n" +
                $"{progressSummary}\n" +
                $"{inventorySummary}\n\n" +
                "【规则】\n" +
                "1. 使用简体中文，严格 1 句话，15～40 字。\n" +
                "2. 语气俏皮、像熟人开玩笑，但不要冒犯玩家。\n" +
                "3. 可以提到背包里的真实物品；背包为空可调侃两手空空。\n" +
                "4. 只能引用【游戏进度】里已有的信息，不要剧透。\n" +
                "5. 只输出台词正文，不要引号、旁白或「小妹说：」。";
        }

        public string BuildItemInterpretPrompt(ItemData item)
        {
            if (item == null)
                return "你是《两界》中的占卜师许负。当前没有选中物品，请简短说明需要先选择物品。";

            return
                "你是汉代占卜师许负，为《两界》中的物品撰写解读。\n" +
                $"物品：{item.DisplayName}（{item.ItemId}）\n" +
                $"类型：{item.ItemType}\n" +
                $"官方描述：{item.Description}\n\n" +
                "请输出 2-3 句话：文言与白话混合，并给一个不剧透的探索提示。\n" +
                "不要超出上述设定。";
        }
    }
}
