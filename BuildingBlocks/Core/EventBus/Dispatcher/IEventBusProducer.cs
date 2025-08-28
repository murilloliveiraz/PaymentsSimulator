namespace BuildingBlocks.Core.EventBus.Dispatcher
{
    public interface IEventBusProducer<in TEvent>
    {
        Task PublishAsync(string topic, string key, TEvent @event, CancellationToken cancellationToken = default);
    }

}
