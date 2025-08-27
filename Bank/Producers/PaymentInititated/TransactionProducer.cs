using BuildingBlocks.Core.EventBus.Events;

namespace Bank.Producers.PaymentInititated
{
    public class TransactionProducer
    {
        public async Task ProduceNewPayment(PaymentInitiatedEvent payment)
        {
            using (var dispatcher = new KafkaDispatcher<string, PaymentInitiatedEvent>(Serializers.Utf8, new JsonSerializer<PaymentInitiatedEvent>()))
            {
                await dispatcher.SendAsync(QueueNames.GPay.InitiatePayment, payment.Utr, payment);
            }
        }
    }
}
