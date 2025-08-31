namespace BuildingBlocks.Core.EventBus.Events
{
    public record RefundSuccessEvent(
        string TransactionId,
        string Utr,
        string Status,
        DateTime TransactionDate);
}
