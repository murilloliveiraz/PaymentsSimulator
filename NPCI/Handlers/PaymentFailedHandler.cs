using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using NPCI.Repository;
using System.Text.Json;

namespace NFCI.Handlers
{
    public class PaymentFailedHandler : IEventHandler<PaymentFailedEvent>
    {
        private readonly ILogger<PaymentFailedEvent> _logger;
        private readonly IPaymentsRepository _paymentRepository;
        private readonly IOutboxRepository _outboxRepository;

        public PaymentFailedHandler(ILogger<PaymentFailedEvent> logger, IPaymentsRepository paymentRepository, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentFailedEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            var paymentFailedEvent = new PaymentFailedEvent(
                @event.TransactionId,
                @event.Utr,
                @event.SenderAccount,
                @event.ReceiverAccount,
                @event.Amount,
                PaymentStatuses.PaymentFailed,
                DateTime.UtcNow,
                @event.Reason
            );
            
            var refundRequestEvent = new RefundRequestEvent(
                @event.TransactionId,
                @event.Utr,
                RefundStatus.Pending,
                DateTime.UtcNow
            );

            await _paymentRepository.UpdateStatus(@event.Utr, PaymentStatuses.RefundRequested);

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                CorrelationId = paymentFailedEvent.Utr,
                Topic = QueueNames.GPay.PaymentFailed,
                EventType = nameof(PaymentFailedEvent),
                Payload = JsonSerializer.Serialize(paymentFailedEvent),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            
            await _outboxRepository.AddAsync(new OutboxMessage
            {
                CorrelationId = paymentFailedEvent.Utr,
                Topic = QueueNames.Bank.RefundRequest,
                EventType = nameof(RefundRequestEvent),
                Payload = JsonSerializer.Serialize(refundRequestEvent),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
