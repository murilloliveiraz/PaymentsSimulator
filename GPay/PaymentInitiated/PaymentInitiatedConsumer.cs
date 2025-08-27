using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Helpers;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Core.Services;
using Confluent.Kafka;

namespace GPay.PaymentInitiated
{
    public class PaymentInitiatedConsumer : IConsumerFunction<string, PaymentInitiatedEvent>
    {
        private readonly PaymentInitiatedProducer _paymentProducer;
        private readonly ILogger<PaymentInitiatedEvent> _logger;

        public PaymentInitiatedConsumer(PaymentInitiatedProducer paymentProducer, ILogger<PaymentInitiatedEvent> logger)
        {
            _paymentProducer = paymentProducer;
            _logger = logger;
        }

        public async Task Consume(ConsumeResult<string, PaymentInitiatedEvent> record)
        {
            var tx = record.Message.Value;
            _logger.LogInformation("Processing PaymentInitiatedEvent [TransactionId={TransactionId}, UTR={UTR}]", tx.TransactionId, tx.Utr);

            if (tx.Amount > 0)
            {
                await _paymentProducer.ProcessDebitRequestNPCI(tx);
            }
            else
            {
                await _paymentProducer.ProducePaymentFailed(new PaymentFailedEvent(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, PaymentStatuses.PaymentFailed, DateTime.UtcNow));
            }
        }

        public Task Start(CancellationToken cancellationToken) =>
           new KafkaService<string, PaymentInitiatedEvent>(
               "PaymentInitiatedConsumer",
               QueueNames.GPay.InitiatePayment,
               this,
               Deserializers.Utf8,
               new JsonDeserializer<PaymentInitiatedEvent>()
           ).Run(cancellationToken);
    }
}
