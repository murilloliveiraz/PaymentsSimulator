namespace BuildingBlocks.Core.EventBus.Events
{
    public record CreditSuccessEvent(
        int TransactionId,
        string Utr,
        string SenderAccount,
        string ReceiverAccount,
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
