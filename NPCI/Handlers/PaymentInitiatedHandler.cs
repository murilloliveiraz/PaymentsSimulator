using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using NPCI.Models;
using NPCI.Repository;
using System.Text.Json;

namespace NFCI.Handlers
{
    public class PaymentInitiatedHandler : IEventHandler<PaymentInitiatedEvent>
    {
        private readonly ILogger<PaymentInitiatedEvent> _logger;
        private readonly PaymentsRepository _paymentRepository;
        private readonly OutboxRepository _outboxRepository;

        public PaymentInitiatedHandler(ILogger<PaymentInitiatedEvent> logger, PaymentsRepository paymentRepository, OutboxRepository outboxRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(PaymentInitiatedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentInitiatedEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            var debitEvent = new DebitRequestEvent(
                @event.TransactionId,
                @event.Utr,
                @event.SenderAccount,
                @event.ReceiverAccount,
                @event.Amount,
                PaymentStatuses.DebitRequested,
                DateTime.UtcNow
            );

            await _paymentRepository.AddAsync(new PaymentSaga
            {
                TransactionId = @event.TransactionId,
                Utr = @event.Utr,
                SenderAccount = @event.SenderAccount,
                ReceiverAccount = @event.ReceiverAccount,
                Amount = @event.Amount,
                Status = PaymentStatuses.Initiated,
                CreatedAt = DateTime.UtcNow
            });

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                CorrelationId = debitEvent.Utr,
                Topic = QueueNames.Bank.DebitRequest,
                EventType = nameof(DebitRequestEvent),
                Payload = JsonSerializer.Serialize(debitEvent),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
