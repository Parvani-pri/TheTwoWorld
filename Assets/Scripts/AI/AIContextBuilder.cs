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
        [SerializeField] string playerCharacterName = "许负";
        [TextArea(2, 3)]
        [SerializeField] string playerCharacterRole = "汉代阴阳师、相馆主人，小妹的搭档。";
        [TextArea(3, 6)]
        [SerializeField] string defaultNpcPersona =
            "你是「小妹」，许负的助手，性格活泼、调皮，熟悉地府与傩面等传说。说话简短口语化，站在许负这边。";

        [Header("Services")]
        [SerializeField] GameProgress gameProgress;

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public string BuildInventorySummary(PlayerInventory inventory, string ownerName = null)
        {
            var owner = string.IsNullOrWhiteSpace(ownerName) ? "玩家" : ownerName.Trim();

            if (inventory == null)
                return $"{owner}背包信息不可用。";

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
                return $"{owner}背包为空。";

            var builder = new StringBuilder();
            builder.Append(owner).Append("当前背包（共 ").Append(itemTotals.Count).Append(" 种物品）：");

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
            var resolvedPlayerName = string.IsNullOrWhiteSpace(playerCharacterName) ? "许负" : playerCharacterName.Trim();
            var inventorySummary = BuildInventorySummary(inventory, resolvedPlayerName);
            var sceneName = BuildSceneContext();
            var progressSummary = BuildProgressSummary(progressLevel);

            return
                $"你是《两界》(TheTwoWorld) 中的「{resolvedName}」。\n" +
                $"{resolvedPersona}\n" +
                $"与你对话的人是{resolvedPlayerName}——{playerCharacterRole}\n\n" +
                "【当前状态】\n" +
                $"场景：{sceneName}\n" +
                $"{progressSummary}\n" +
                $"{inventorySummary}\n\n" +
                "【回复规则】\n" +
                "1. 使用简体中文，每次 1-3 句话，口语化。\n" +
                $"2. 你在对{resolvedPlayerName}说话，可用「你」或直接喊他名字。\n" +
                "3. 禁止使用「客官」「这位客人」「阁下」等招待顾客的称呼。\n" +
                $"4. 可以引用{resolvedPlayerName}背包中的物品名进行回答。\n" +
                $"5. 不要编造{resolvedPlayerName}没有的物品。\n" +
                "6. 只能引用【游戏进度】中已出现的内容，不要剧透未解锁剧情。\n" +
                "7. 不确定的问题回答「我不太清楚」。\n" +
                "8. 未在进度中出现的内容，请用角色口吻回避。";
        }

        public string BuildInterruptPrompt(PlayerInventory inventory, string personaOverride = null)
        {
            var persona = string.IsNullOrWhiteSpace(personaOverride) ? defaultNpcPersona : personaOverride.Trim();
            var resolvedPlayerName = string.IsNullOrWhiteSpace(playerCharacterName) ? "许负" : playerCharacterName.Trim();
            var inventorySummary = BuildInventorySummary(inventory, resolvedPlayerName);
            var sceneName = BuildSceneContext();
            var progressSummary = BuildProgressSummary(AIContextLevel.Compact);

            return
                "你是《两界》中的「小妹」，许负身边的助手，性格活泼、调皮。\n" +
                $"{persona}\n" +
                $"你面前站着的就是{resolvedPlayerName}——{playerCharacterRole}\n" +
                $"这不是招待客人，而是你追上{resolvedPlayerName}，想当面随口插一句话。\n\n" +
                "【对话对象】\n" +
                $"1. 你在对{resolvedPlayerName}说话，可用「你」或直接喊他名字。\n" +
                "2. 禁止使用「客官」「这位客人」「阁下」「来客」等招待顾客的称呼。\n" +
                "3. 语气像熟人拌嘴、吐槽搭档，不是掌柜揽客。\n\n" +
                "【当前状态】\n" +
                $"场景：{sceneName}\n" +
                $"{progressSummary}\n" +
                $"{inventorySummary}\n\n" +
                "【规则】\n" +
                "1. 使用简体中文，严格 1 句话，15～40 字。\n" +
                $"2. 语气俏皮、像对{resolvedPlayerName}开玩笑，但不要冒犯他。\n" +
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
