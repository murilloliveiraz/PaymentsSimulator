namespace Bank.Models.EventsResponse
{
    public class RefundResult
    {
        public int RefundId { get; }
        public int TransactionId { get; }
        public string Utr { get; }
        public string Status { get; }
        public string? ErrorMessage { get; }
        public bool IsSuccess => Status == PaymentStatuses.RefundSuccess;

        private RefundResult(
            int refundId,
            int transactionId,
            string utr,
            string status,
            string? errorMessage = null)
        {
            RefundId = refundId;
            TransactionId = transactionId;
            Utr = utr;
            Status = status;
            ErrorMessage = errorMessage;
        }

        public static RefundResult Success(
            int refundId,
            int transactionId,
            string utr) =>
            new(refundId, transactionId, utr, PaymentStatuses.RefundSuccess);

        public static RefundResult Failed(
            int refundId,
            int transactionId,
            string utr,
            string errorMessage) =>
            new(refundId, transactionId, utr, PaymentStatuses.RefundFailed, errorMessage);
    }
}
