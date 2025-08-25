namespace BuildingBlocks.Core.EventBus.Events
{
    public record CreditRequestEvent(
        string TransactionId, 
        string Utr, 
        string SenderAccount, 
        string ReceiverAccount, 
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
