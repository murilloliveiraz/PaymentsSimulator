namespace BuildingBlocks.Core.EventBus.Events
{
    public record RefundRequestEvent(
        int TransactionId,
        string Utr,
        string Status,
        DateTime TransactionDate);
}
