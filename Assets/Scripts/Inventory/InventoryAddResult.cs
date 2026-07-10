namespace TwoWorlds.Inventory
{
    public enum InventoryAddStatus
    {
        Success,
        Partial,
        Failed
    }

    public readonly struct InventoryAddResult
    {
        public InventoryAddStatus Status { get; }
        public int AddedAmount { get; }
        public int RequestedAmount { get; }

        public bool IsFullSuccess => Status == InventoryAddStatus.Success;

        InventoryAddResult(InventoryAddStatus status, int addedAmount, int requestedAmount)
        {
            Status = status;
            AddedAmount = addedAmount;
            RequestedAmount = requestedAmount;
        }

        public static InventoryAddResult Success(int amount) =>
            new(InventoryAddStatus.Success, amount, amount);

        public static InventoryAddResult Partial(int addedAmount, int requestedAmount) =>
            new(InventoryAddStatus.Partial, addedAmount, requestedAmount);

        public static InventoryAddResult Failed(int requestedAmount) =>
            new(InventoryAddStatus.Failed, 0, requestedAmount);
    }
}
