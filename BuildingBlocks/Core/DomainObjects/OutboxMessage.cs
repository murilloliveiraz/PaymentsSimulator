using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Core.DomainObjects
{
    public class OutboxMessage
    {
        [Key]
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; }
        public string Topic { get; set; }
        public string Payload { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; } = OutboxStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
    }
}
