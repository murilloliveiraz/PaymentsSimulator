namespace BuildingBlocks.Core.EventBus.Events
{
    public record DebitFailedEvent(
        string TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
