using System.Collections.Generic;
using XuFu.MaskSystem;

namespace TwoWorlds.Progress
{
    public static class EnterYinMaskRequirements
    {
        static readonly string[] Chapter2Required = { MaskEquipGate.ZhongKuiItemId };
        static readonly string[] Chapter3Required = { MaskEquipGate.ZhongKuiItemId, MaskEquipGate.BoqiItemId };

        public static IReadOnlyList<string> GetRequiredItemIds(int chapter)
        {
            switch (chapter)
            {
                case 2:
                    return Chapter2Required;
                case 3:
                    return Chapter3Required;
                default:
                    return System.Array.Empty<string>();
            }
        }
    }
}
