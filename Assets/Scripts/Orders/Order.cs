namespace AquariumShop
{
    public enum OrderStatus
    {
        Open,
        Done,
        Failed,
        Refused,
        Held
    }

    [System.Serializable]
    public class Order
    {
        public string id;
        public FishSpecies wanted;
        public int qty = 1;
        public int qtyFilled;
        public float payout;
        public float penalty;
        public float hoursRemaining;
        public OrderStatus status = OrderStatus.Open;

        public bool IsOpen => status == OrderStatus.Open;
        public int QtyLeft => qty - qtyFilled;
        public float PayoutPerFish => qty > 0 ? payout / qty : payout;
    }
}