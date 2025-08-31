namespace BuildingBlocks.Core.EventBus.Events
{
    public record PaymentInitiatedEvent(
        int TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
