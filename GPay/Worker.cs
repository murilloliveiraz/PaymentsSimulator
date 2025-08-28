using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.EventBus;

namespace GPay
{
    public class Worker : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public Worker(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<PaymentInitiatedEvent, PaymentInitiatedHandler>(QueueNames.GPay.InitiatePayment);

            return Task.CompletedTask;
        }
    }
}
