using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.Core.DomainObjects
{
    public class Refund
    {
        [Key]
        public int RefundId { get; set; }
        public int TransactionId { get; set; }
        public string Utr { get; set; }
        public string Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime RefundedAt { get; set; }
        public DateTime LastAttemptAt { get; set; }
        public DateTime NextAttemptAt { get; set; }
        public int RetryCount { get; set; }
    }
}
