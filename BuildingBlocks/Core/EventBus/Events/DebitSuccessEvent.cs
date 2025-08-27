namespace BuildingBlocks.Core.EventBus.Events
{
    public record DebitSuccessEvent(
        string TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
