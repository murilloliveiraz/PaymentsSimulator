namespace BuildingBlocks.Core.DomainObjects
{
    public class Transaction
    {
        public string TransactionId { get; set; }
        public string Utr { get; set; }
        public string SenderAccount { get; set; }
        public string ReceiverAccount { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
