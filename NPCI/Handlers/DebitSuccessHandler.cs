using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using NPCI.Repository;
using System.Text.Json;

namespace NFCI.Handlers
{
    public class DebitSuccessHandler : IEventHandler<DebitSuccessEvent>
    {
        private readonly ILogger<DebitSuccessEvent> _logger;
        private readonly IPaymentsRepository _paymentRepository;
        private readonly IOutboxRepository _outboxRepository;

        public DebitSuccessHandler(ILogger<DebitSuccessEvent> logger, IPaymentsRepository paymentRepository, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(DebitSuccessEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing DebitSuccessEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            var CreditEvent = new CreditRequestEvent(
                @event.TransactionId,
                @event.Utr,
                @event.SenderAccount,
                @event.ReceiverAccount,
                @event.Amount,
                PaymentStatuses.CreditRequested,
                DateTime.UtcNow
            );

            await _paymentRepository.UpdateStatus(@event.Utr, PaymentStatuses.DebitSuccess);

            _logger.LogInformation("Adding CreditRequestEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                CorrelationId = CreditEvent.Utr,
                Topic = QueueNames.Bank.CreditRequest,
                EventType = nameof(CreditRequestEvent),
                Payload = JsonSerializer.Serialize(CreditEvent),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
