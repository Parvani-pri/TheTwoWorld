using System.Collections.Generic;
using System.Text;
using TwoWorlds.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwoWorlds.AI
{
    public class AIContextBuilder : MonoBehaviour
    {
        [Header("NPC Persona")]
        [SerializeField] string defaultNpcName = "小梅";
        [TextArea(3, 6)]
        [SerializeField] string defaultNpcPersona =
            "你是汉代阴阳师的助手，性格温和，说话简短，熟悉地府与傩面相关传说。";

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

        public string BuildNpcSystemPrompt(string npcName, string persona, PlayerInventory inventory)
        {
            var resolvedName = string.IsNullOrWhiteSpace(npcName) ? defaultNpcName : npcName.Trim();
            var resolvedPersona = string.IsNullOrWhiteSpace(persona) ? defaultNpcPersona : persona.Trim();
            var inventorySummary = BuildInventorySummary(inventory);
            var sceneName = BuildSceneContext();

            return
                $"你是《两界》(TheTwoWorld) 中的 NPC「{resolvedName}」。\n" +
                $"{resolvedPersona}\n\n" +
                "【当前状态】\n" +
                $"场景：{sceneName}\n" +
                $"{inventorySummary}\n\n" +
                "【回复规则】\n" +
                "1. 使用简体中文，每次 1-3 句话，口语化。\n" +
                "2. 可以引用玩家背包中的物品名进行回答。\n" +
                "3. 不要编造玩家没有的物品。\n" +
                "4. 不要剧透未解锁的主线剧情。\n" +
                "5. 不确定的问题回答「我不太清楚」。";
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
