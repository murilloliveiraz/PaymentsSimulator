using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using NPCI.Repository;
using System.Text.Json;

namespace NFCI.Handlers
{
    public class PaymentSuccessHandler : IEventHandler<PaymentSuccessEvent>
    {
        private readonly ILogger<PaymentSuccessEvent> _logger;
        private readonly IPaymentsRepository _paymentRepository;
        private readonly IOutboxRepository _outboxRepository;

        public PaymentSuccessHandler(ILogger<PaymentSuccessEvent> logger, IPaymentsRepository paymentRepository, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(PaymentSuccessEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentSuccessEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            var paymentSuccessEvent = new PaymentSuccessEvent(
                @event.TransactionId,
                @event.Utr,
                @event.SenderAccount,
                @event.ReceiverAccount,
                @event.Amount,
                PaymentStatuses.PaymentSuccess,
                DateTime.UtcNow
            );

            await _paymentRepository.UpdateStatus(@event.Utr, PaymentStatuses.PaymentSuccess);

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                CorrelationId = paymentSuccessEvent.Utr,
                Topic = QueueNames.GPay.PaymentSuccess,
                EventType = nameof(PaymentSuccessEvent),
                Payload = JsonSerializer.Serialize(paymentSuccessEvent),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
