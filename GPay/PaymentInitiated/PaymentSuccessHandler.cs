using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
namespace GPay
{
    public class PaymentSuccessHandler : IEventHandler<PaymentSuccessEvent>
    {
        private readonly IEventBusProducer<string> _eventBus;
        private readonly ILogger<PaymentSuccessHandler> _logger;

        public PaymentSuccessHandler(ILogger<PaymentSuccessHandler> logger, IEventBusProducer<string> eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(PaymentSuccessEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[GPay] Received PaymentSuccessEvent [TransactionId={TransactionId}, UTR={UTR}]",
                @event.TransactionId, @event.Utr);
        }
    }
}
