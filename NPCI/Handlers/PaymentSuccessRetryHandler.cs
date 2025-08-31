using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using NPCI.Repository;
using System.Text.Json;

namespace NFCI.Handlers
{
    public class PaymentSuccessRetryHandler : IEventHandler<PaymentSuccessEvent>
    {
        private readonly ILogger<PaymentSuccessEvent> _logger;
        private readonly IPaymentsRepository _paymentRepository;
        private readonly IOutboxRepository _outboxRepository;
        const int MAX_RETRIES = 3;

        public PaymentSuccessRetryHandler(ILogger<PaymentSuccessEvent> logger, IPaymentsRepository paymentRepository, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task HandleAsync(PaymentSuccessEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentSuccessEvent Retry [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            var pastMessages = await _outboxRepository.GetMessagesByCorrelationIdAndStatus(@event.Utr, @event.Status);

            var successEvent = new PaymentSuccessEvent(
                @event.TransactionId,
                @event.Utr,
                @event.SenderAccount,
                @event.ReceiverAccount,
                @event.Amount,
                PaymentStatuses.CreditSuccess,
                DateTime.UtcNow
            );

            if (pastMessages.Count() >= MAX_RETRIES)
            {
                var paymentFailedEvent = new PaymentFailedEvent(
                    @event.TransactionId,
                    @event.Utr,
                    @event.SenderAccount,
                    @event.ReceiverAccount,
                    @event.Amount,
                    PaymentStatuses.PaymentFailed,
                    DateTime.UtcNow,
                    "Max retries reached"
                );

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
                    CorrelationId = successEvent.Utr,
                    Topic = QueueNames.Bank.PaymentSuccess + ".DLQ",
                    EventType = nameof(PaymentSuccessEvent),
                    Payload = JsonSerializer.Serialize(successEvent),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {

                _logger.LogWarning(
                    "Retrying PaymentSuccessEvent [UTR={Utr}, Attempt={Attempt}/{MaxRetries}]",
                    @event.Utr, pastMessages.Count() + 1, MAX_RETRIES);

                await _paymentRepository.UpdateStatus(@event.Utr, PaymentStatuses.CreditSuccess);

                await _outboxRepository.AddAsync(new OutboxMessage
                {
                    CorrelationId = successEvent.Utr,
                    Topic = QueueNames.Bank.PaymentSuccess,
                    EventType = nameof(PaymentSuccessEvent),
                    Payload = JsonSerializer.Serialize(successEvent),
                    Status = OutboxStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}
