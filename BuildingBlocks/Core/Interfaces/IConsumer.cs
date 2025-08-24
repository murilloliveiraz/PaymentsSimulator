namespace BuildingBlocks.Core.Interfaces
{
    public interface IConsumerFunction<TKey, TValue>
    {
        void Consume(ConsumeResult<TKey, TValue> record);
    }
}
