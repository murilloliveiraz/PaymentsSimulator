using BuildingBlocks.Core.EventBus;
using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;

namespace GPay
{
    public class PaymentInitiatedProducer
    {
        private readonly KafkaDispatcher<string, PaymentInitiatedEvent> _debitDispatcher;
        private readonly KafkaDispatcher<string, PaymentFailedEvent> _failedDispatcher;

        public PaymentInitiatedProducer(
            KafkaDispatcher<string, PaymentInitiatedEvent> debitDispatcher,
            KafkaDispatcher<string, PaymentFailedEvent> failedDispatcher
            )
        {
            _debitDispatcher = debitDispatcher;
            _failedDispatcher = failedDispatcher;
        }

        public Task ProcessDebitRequestNPCI(PaymentInitiatedEvent transaction) =>
            _debitDispatcher.SendAsync(QueueNames.NPCI.DebitRequest, transaction.Utr, transaction);

        public Task ProducePaymentFailed(PaymentFailedEvent transaction) =>
            _failedDispatcher.SendAsync(QueueNames.GPay.PaymentFailed, transaction.Utr, transaction);
    }
}
