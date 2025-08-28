using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using System.Text.Json;

namespace Bank.Consumers
{
    public class DebitRequestHandler : IEventHandler<DebitRequestEvent>
    {
        private readonly ILogger<DebitRequestEvent> _logger;
        private readonly DebitService _debitService;
        private readonly IOutboxRepository _outboxRepository;

        public DebitRequestHandler(ILogger<DebitRequestEvent> logger, DebitService debitService, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _debitService = debitService;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(DebitRequestEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing DebitRequestEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            try
            {
                var result = await _debitService.HandleDebitRequest(@event);

                object domainEvent = result.IsSuccess
                    ? new DebitSuccessEvent(result.TransactionId, result.Utr, result.SenderAccount, result.ReceiverAccount, result.Amount, PaymentStatuses.DebitSuccess, DateTime.UtcNow)
                    : new PaymentFailedEvent(result.TransactionId, result.Utr, result.SenderAccount, result.ReceiverAccount, result.Amount, PaymentStatuses.PaymentFailed, DateTime.UtcNow, result.ErrorMessage);

                var outboxEvent = new OutboxMessage
                {
                    CorrelationId = @event.Utr,
                    Topic = result.IsSuccess ? QueueNames.NPCI.DebitSuccess : QueueNames.NPCI.PaymentFailed,
                    EventType = result.IsSuccess ? nameof(DebitSuccessEvent) : nameof(PaymentFailedEvent),
                    Payload = JsonSerializer.Serialize(domainEvent),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(outboxEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Infra error processing debit");

                var outboxEvent = new OutboxMessage
                {
                    CorrelationId = @event.Utr,
                    Topic = QueueNames.NPCI.DebitRequest,
                    EventType = nameof(DebitRequestEvent),
                    Payload = JsonSerializer.Serialize(@event),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(outboxEvent);
            }
        }
    }
}
