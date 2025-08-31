namespace BuildingBlocks.Core.EventBus.Events
{
    public record CreditRequestEvent(
        int TransactionId, 
        string Utr, 
        string SenderAccount, 
        string ReceiverAccount, 
        decimal Amount,
        string Status,
        DateTime TransactionDate);
}
