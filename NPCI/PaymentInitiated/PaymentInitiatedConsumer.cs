using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Helpers;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Core.Services;
using Confluent.Kafka;
using GPay;
using NPCI.Models;
using System.Text.Json;

namespace NFCI.PaymentInitiated
{
    public class PaymentInitiatedConsumer : IConsumerFunction<string, PaymentInitiatedEvent>
    {
        private readonly ILogger<PaymentInitiatedEvent> _logger;

        public PaymentInitiatedConsumer(ILogger<PaymentInitiatedEvent> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeResult<string, PaymentInitiatedEvent> record)
        {
            var tx = record.Message.Value;
            _logger.LogInformation("Processing PaymentInitiatedEvent [TransactionId={TransactionId}, UTR={UTR}]", tx.TransactionId, tx.Utr);

            await _paymentRepository.AddAsync(new PaymentSaga
            {
                TransactionId = tx.TransactionId,
                Utr = tx.Utr,
                SenderAccount = tx.SenderAccount,
                ReceiverAccount = tx.ReceiverAccount,
                Amount = tx.Amount,
                Status = PaymentStatuses.Initiated,
                LastUpdated = DateTime.UtcNow
            });

            var debitEvent = new DebitRequestEvent(
                tx.TransactionId,
                tx.Utr,
                tx.SenderAccount,
                tx.ReceiverAccount,
                tx.Amount,
                PaymentStatuses.DebitRequested,
                DateTime.UtcNow
            );

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                CorrelationId = debitEvent.Utr,
                EventType = nameof(DebitRequestEvent),
                Payload = JsonSerializer.Serialize(debitEvent),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

        }

        public async Task Start(CancellationToken cancellationToken)
        {
            using (var kafkaService = new KafkaService<string, PaymentInitiatedEvent>(
                "PaymentInitiatedConsumer",
                QueueNames.GPay.InitiatePayment,
                this,
                Deserializers.Utf8,
                new JsonDeserializer<PaymentInitiatedEvent>()
                ))
            {
                await kafkaService.Run(cancellationToken);
            }
        }
    }
}
