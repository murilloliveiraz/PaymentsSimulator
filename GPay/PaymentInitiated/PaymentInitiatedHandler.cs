using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
namespace GPay
{
    public class PaymentInitiatedHandler : IEventHandler<PaymentInitiatedEvent>
    {
        private readonly IEventBusProducer<object> _eventBus;
        private readonly ILogger<PaymentInitiatedHandler> _logger;

        public PaymentInitiatedHandler(ILogger<PaymentInitiatedHandler> logger, IEventBusProducer<object> eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(PaymentInitiatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentInitiatedEvent [TransactionId={TransactionId}, UTR={UTR}]",
                @event.TransactionId, @event.Utr);

            if (@event.Amount > 0)
            {
                await _eventBus.PublishAsync(QueueNames.NPCI.DebitRequest, @event.Utr, @event, cancellationToken);
            }
            else
            {
                var failed = new PaymentFailedEvent(
                    @event.TransactionId, @event.Utr,
                    @event.SenderAccount, @event.ReceiverAccount,
                    @event.Amount, PaymentStatuses.PaymentFailed, DateTime.UtcNow, "Amount cannot be negative");

                await _eventBus.PublishAsync(QueueNames.GPay.PaymentFailed, @event.Utr, failed, cancellationToken);
            }
        }
    }
}
