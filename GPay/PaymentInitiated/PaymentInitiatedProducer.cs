using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Helpers;
using Confluent.Kafka;

namespace GPay
{
    public class PaymentInitiatedProducer
    {
        public async Task ProcessPaymentNPCI(PaymentInitiatedEvent transaction)
        {
            using (var dispatcher = new KafkaDispatcher<string, PaymentInitiatedEvent>(Serializers.Utf8, new JsonSerializer<PaymentInitiatedEvent>()))
            {
                await dispatcher.SendAsync(QueueNames.NPCI.DebitRequest, transaction.Utr, transaction);
            }
        }

        public async Task ProducePaymentFailed(PaymentInitiatedEvent transaction)
        {
            using (var dispatcher = new KafkaDispatcher<string, PaymentInitiatedEvent>(Serializers.Utf8, new JsonSerializer<PaymentInitiatedEvent>()))
            {
                await dispatcher.SendAsync(QueueNames.GPay.PaymentFailed, transaction.Utr, transaction);
            }
        }
    }
}
