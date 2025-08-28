namespace BuildingBlocks.Core.EventBus.Dispatcher
{
    public interface IEventBus
    {
        void Subscribe<TEvent, THandler>(string topic)
            where TEvent : class
            where THandler : IEventHandler<TEvent>;
    }
}
