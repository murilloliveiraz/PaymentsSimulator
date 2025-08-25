using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Helpers;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Core.Services;
using Confluent.Kafka;

namespace NFCI.PaymentInitiated
{
    public class PaymentInitiatedConsumer : IConsumerFunction<string, PaymentInitiatedEvent>
    {
        private readonly PaymentInitiatedProducer _paymentProducer;

        public PaymentInitiatedConsumer(PaymentInitiatedProducer paymentProducer)
        {
            _paymentProducer = paymentProducer;
        }

        public async void Consume(ConsumeResult<string, PaymentInitiatedEvent> record)
        {
            Console.WriteLine($"[PaymentInitiated] Processing: {record.Message.Key}");

            PaymentInitiatedEvent tx = record.Message.Value;

            if (tx.Amount > 0)
            {
                await _paymentProducer.ProcessPaymentNPCI(tx);
            }
            else
            {
                await _paymentProducer.ProducePaymentFailed(tx);
            }
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
