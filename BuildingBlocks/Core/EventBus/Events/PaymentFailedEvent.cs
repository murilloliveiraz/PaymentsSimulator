namespace BuildingBlocks.Core.EventBus.Events
{
    public record PaymentFailedEvent(
        string TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate,
        string Reason
        );
}
