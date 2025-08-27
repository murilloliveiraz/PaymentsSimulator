using BuildingBlocks.Core.EventBus.Events;

namespace Bank.Producers.DebitRequest
{
    public class DebitRequestProducer
    {
        private readonly KafkaDispatcher<string, DebitSuccessEvent> _successDispatcher;
        private readonly KafkaDispatcher<string, DebitFailedEvent> _failedDispatcher;
        private readonly KafkaDispatcher<string, DebitRequestEvent> _retryDispatcher;
        private readonly KafkaDispatcher<string, DebitRequestEvent> _dlqDispatcher;

        public DebitRequestProducer(
            KafkaDispatcher<string, DebitSuccessEvent> successDispatcher,
            KafkaDispatcher<string, DebitFailedEvent> failedDispatcher,
            KafkaDispatcher<string, DebitRequestEvent> retryDispatcher,
            KafkaDispatcher<string, DebitRequestEvent> dlqDispatcher)
        {
            _successDispatcher = successDispatcher;
            _failedDispatcher = failedDispatcher;
            _retryDispatcher = retryDispatcher;
            _dlqDispatcher = dlqDispatcher;
        }

        public Task ProcessDebitSuccess(DebitSuccessEvent transaction) =>
            _successDispatcher.SendAsync(QueueNames.NPCI.DebitSuccess, transaction.Utr, transaction);

        public Task ProduceDebitFailed(DebitFailedEvent transaction) =>
            _failedDispatcher.SendAsync(QueueNames.NPCI.DebitFailed, transaction.Utr, transaction);

        public Task SendDebitRequestToDLQ(DebitRequestEvent transaction) =>
            _dlqDispatcher.SendAsync(QueueNames.NPCI.DebitDLQ, transaction.Utr, transaction);

        public Task ProduceDebitRetry(DebitRequestEvent transaction) =>
            _retryDispatcher.SendAsync(QueueNames.Bank.DebitRequest, transaction.Utr, transaction);
    }
}
