using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using System.Text.Json;
namespace GPay
{
    public class PaymentInitiatedHandler : IEventHandler<PaymentInitiatedEvent>
    {
        private readonly IEventBusProducer<string> _eventBus;
        private readonly ILogger<PaymentInitiatedHandler> _logger;

        public PaymentInitiatedHandler(ILogger<PaymentInitiatedHandler> logger, IEventBusProducer<string> eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(PaymentInitiatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[GPay] Processing PaymentInitiatedEvent [TransactionId={TransactionId}, UTR={UTR}]",
                @event.TransactionId, @event.Utr);

            if (@event.Amount > 0)
            {
                var payload = JsonSerializer.Serialize(@event);
                await _eventBus.PublishAsync(QueueNames.NPCI.PaymentInitiated, @event.Utr, payload, cancellationToken);
            }
            else
            {
                var failed = new PaymentFailedEvent(
                    @event.TransactionId, @event.Utr,
                    @event.SenderAccount, @event.ReceiverAccount,
                    @event.Amount, PaymentStatuses.PaymentFailed, DateTime.UtcNow, "Amount cannot be negative");

                var payload = JsonSerializer.Serialize(failed);

                await _eventBus.PublishAsync(QueueNames.GPay.PaymentFailed, @event.Utr, payload, cancellationToken);
            }
        }
    }
}
