namespace BuildingBlocks.Core.DomainObjects
{
    public class TransactionEvent
    {
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public string EventType { get; set; }
        public Transaction EventData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
