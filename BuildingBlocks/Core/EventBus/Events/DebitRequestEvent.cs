namespace BuildingBlocks.Core.EventBus.Events
{
    public record DebitRequestEvent(
        string TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
