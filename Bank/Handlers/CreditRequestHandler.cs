using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using System.Text.Json;

namespace Bank.Consumers
{
    public class CreditRequestHandler : IEventHandler<CreditRequestEvent>
    {
        private readonly ILogger<CreditRequestEvent> _logger;
        private readonly CreditService _CreditService;
        private readonly IOutboxRepository _outboxRepository;

        public CreditRequestHandler(ILogger<CreditRequestEvent> logger, CreditService CreditService, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _CreditService = CreditService;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(CreditRequestEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing CreditRequestEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            try
            {
                var result = await _CreditService.HandleCreditRequest(@event);

                object domainEvent = result.IsSuccess
                    ? new CreditSuccessEvent(result.TransactionId, result.Utr, result.SenderAccount, result.ReceiverAccount, result.Amount, PaymentStatuses.CreditSuccess, DateTime.UtcNow)
                    : new PaymentFailedEvent(result.TransactionId, result.Utr, result.SenderAccount, result.ReceiverAccount, result.Amount, PaymentStatuses.PaymentFailed, DateTime.UtcNow, result.ErrorMessage);

                var outboxEvent = new OutboxMessage
                {
                    CorrelationId = @event.Utr,
                    Topic = result.IsSuccess ? QueueNames.NPCI.CreditSuccess : QueueNames.NPCI.PaymentFailed,
                    EventType = result.IsSuccess ? nameof(CreditSuccessEvent) : nameof(PaymentFailedEvent),
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
                    Topic = QueueNames.NPCI.CreditRequest,
                    EventType = nameof(CreditRequestEvent),
                    Payload = JsonSerializer.Serialize(@event),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(outboxEvent);
            }
        }
    }
}
