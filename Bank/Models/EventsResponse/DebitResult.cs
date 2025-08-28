namespace Bank.Models.EventsResponse
{
    public class DebitResult
    {
        public string TransactionId { get; }
        public string Utr { get; }
        public string SenderAccount { get; }
        public string ReceiverAccount { get; }
        public decimal Amount { get; }
        public string Status { get; }
        public string? ErrorMessage { get; }
        public bool IsSuccess => Status == PaymentStatuses.DebitSuccess;

        private DebitResult(
            string transactionId,
            string utr,
            string senderAccount,
            string receiverAccount,
            decimal amount,
            string status,
            string? errorMessage = null)
        {
            TransactionId = transactionId;
            Utr = utr;
            SenderAccount = senderAccount;
            ReceiverAccount = receiverAccount;
            Amount = amount;
            Status = status;
            ErrorMessage = errorMessage;
        }

        public static DebitResult Success(
            string transactionId,
            string utr,
            string senderAccount,
            string receiverAccount,
            decimal amount) =>
            new(transactionId, utr, senderAccount, receiverAccount, amount, PaymentStatuses.DebitSuccess);

        public static DebitResult Failed(
            string transactionId,
            string utr,
            string senderAccount,
            string receiverAccount,
            decimal amount,
            string errorMessage) =>
            new(transactionId, utr, senderAccount, receiverAccount, amount, PaymentStatuses.DebitFailed, errorMessage);
    }
}
