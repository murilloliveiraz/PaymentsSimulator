namespace BuildingBlocks.Core.EventBus.Dispatcher
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
