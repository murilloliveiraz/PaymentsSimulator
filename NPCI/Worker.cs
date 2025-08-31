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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<CreditRequestEvent, CreditRequestHandler>(QueueNames.NPCI.CreditRequest);
            _eventBus.Subscribe<CreditSuccessEvent, CreditSuccessHandler>(QueueNames.NPCI.CreditSuccess);
            _eventBus.Subscribe<DebitRequestEvent, DebitRequestHandler>(QueueNames.NPCI.DebitRequest);
            _eventBus.Subscribe<DebitSuccessEvent, DebitSuccessHandler>(QueueNames.NPCI.DebitSuccess);
            _eventBus.Subscribe<PaymentFailedEvent, PaymentFailedHandler>(QueueNames.NPCI.PaymentFailed);
            _eventBus.Subscribe<PaymentInitiatedEvent, PaymentInitiatedHandler>(QueueNames.NPCI.PaymentInitiated);
            _eventBus.Subscribe<PaymentSuccessEvent, PaymentSuccessHandler>(QueueNames.NPCI.PaymentSuccess);
            _eventBus.Subscribe<PaymentSuccessEvent, PaymentSuccessRetryHandler>(QueueNames.NPCI.PaymentSuccessRetry);

            await Task.Delay(-1, stoppingToken);
        }
    }
}
