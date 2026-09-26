namespace AquariumShop
{
    public enum OrderStatus { Open, Done, Failed }

    [System.Serializable]
    public class Order
    {
        public string id;
        public FishSpecies wanted;
        public int qty = 1;
        public float payout;
        public float hoursRemaining;
        public OrderStatus status = OrderStatus.Open;

        public bool IsOpen => status == OrderStatus.Open;
    }
}