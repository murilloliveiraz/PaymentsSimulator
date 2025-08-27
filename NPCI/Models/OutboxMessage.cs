namespace NPCI.Models
{
    public class OutboxMessage
    {
        public string MessageId { get; set; }
        public string TransactionId { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
