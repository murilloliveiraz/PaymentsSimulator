namespace BuildingBlocks.Core.EventBus.Events
{
    public record PaymentSuccessEvent(
        int TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
