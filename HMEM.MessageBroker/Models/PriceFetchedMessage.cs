namespace HMEM.MessageBroker.Models
{
    public class PriceFetchedMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Symbol { get; set; } = default!;
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
