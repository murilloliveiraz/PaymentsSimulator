using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
namespace GPay
{
    public class PaymentFailedHandler : IEventHandler<PaymentFailedEvent>
    {
        private readonly IEventBusProducer<string> _eventBus;
        private readonly ILogger<PaymentFailedHandler> _logger;

        public PaymentFailedHandler(ILogger<PaymentFailedHandler> logger, IEventBusProducer<string> eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[GPay] Received PaymentFailedEvent [TransactionId={TransactionId}, UTR={UTR}]",
                @event.TransactionId, @event.Utr);
        }
    }
}
