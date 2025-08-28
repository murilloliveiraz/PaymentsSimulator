using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.EventBus;
using NFCI.Handlers;

namespace NPCI
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
            _eventBus.Subscribe<PaymentInitiatedEvent, PaymentInitiatedHandler>(QueueNames.NPCI.PaymentInitiated);

            return Task.CompletedTask;
        }
    }
}
