namespace BuildingBlocks.Core.Interfaces
{
    public interface IConsumerFunction<TKey, TValue>
    {
        Task Consume(ConsumeResult<TKey, TValue> record);
    }
}
