using Bank.Consumers;
using BuildingBlocks.Core.EventBus.Events;

namespace Bank.Workers
{
    public class EventConsumerWorker : BackgroundService
    {
        private readonly IEventBus _eventBus;

        public EventConsumerWorker(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<CreditRequestEvent, CreditRequestHandler>(QueueNames.Bank.CreditRequest);
            _eventBus.Subscribe<DebitRequestEvent, DebitRequestHandler>(QueueNames.Bank.DebitRequest);
            _eventBus.Subscribe<PaymentSuccessEvent, PaymentSuccessHandler>(QueueNames.Bank.PaymentSuccess);
            _eventBus.Subscribe<RefundRequestEvent, RefundRequestHandler>(QueueNames.Bank.RefundRequest);

            await Task.Delay(-1, stoppingToken);
        }
    }
}
