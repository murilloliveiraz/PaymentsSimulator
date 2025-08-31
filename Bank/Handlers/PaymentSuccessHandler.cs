using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using System.Text.Json;

namespace Bank.Consumers
{
    public class PaymentSuccessHandler : IEventHandler<PaymentSuccessEvent>
    {
        private readonly ILogger<PaymentSuccessEvent> _logger;
        private readonly PaymentSuccessService _paymentSuccessService;
        private readonly IOutboxRepository _outboxRepository;

        public PaymentSuccessHandler(ILogger<PaymentSuccessEvent> logger, PaymentSuccessService paymentSuccessService, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _paymentSuccessService = paymentSuccessService;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(PaymentSuccessEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentSuccessEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            try
            {
                var result = await _paymentSuccessService.HandlePaymentSuccess(@event);

                object domainEvent = result.IsSuccess
                    ? new PaymentSuccessEvent(result.TransactionId, result.Utr, result.SenderAccount, result.ReceiverAccount, result.Amount, PaymentStatuses.PaymentSuccess, DateTime.UtcNow)
                    : new PaymentFailedEvent(result.TransactionId, result.Utr, result.SenderAccount, result.ReceiverAccount, result.Amount, PaymentStatuses.PaymentFailed, DateTime.UtcNow, result.ErrorMessage);

                var outboxEvent = new OutboxMessage
                {
                    CorrelationId = @event.Utr,
                    Topic = result.IsSuccess ? QueueNames.NPCI.PaymentSuccess : QueueNames.NPCI.PaymentFailed,
                    EventType = result.IsSuccess ? nameof(PaymentSuccessEvent) : nameof(PaymentFailedEvent),
                    Payload = JsonSerializer.Serialize(domainEvent),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(outboxEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Infra error processing Credit");

                var outboxEvent = new OutboxMessage
                {
                    CorrelationId = @event.Utr,
                    Topic = QueueNames.NPCI.PaymentSuccessRetry,
                    EventType = nameof(PaymentFailedEvent),
                    Payload = JsonSerializer.Serialize(@event),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(outboxEvent);
            }
        }
    }
}
