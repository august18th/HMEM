namespace HMEM.Common.Models
{
    public class PriceEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Symbol { get; set; } = "ETH";
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
