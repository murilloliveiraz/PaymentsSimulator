using Bank.Producers.DebitRequest;
using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Core.Services;

namespace Bank.Consumers
{
    public class DebitRequestConsumer : IConsumerFunction<string, DebitRequestEvent>
    {
        private readonly DebitService _debitService;
        private readonly ILogger<DebitRequestConsumer> _logger;
        private readonly DebitRequestProducer _debitProducer;

        public DebitRequestConsumer(ILogger<DebitRequestConsumer> logger, DebitService debitService, DebitRequestProducer debitProducer)
        {
            _logger = logger;
            _debitService = debitService;
            _debitProducer = debitProducer;
        }

        public async Task Consume(ConsumeResult<string, DebitRequestEvent> record)
        {
            var tx = record.Message.Value;

            try
            {
                _logger.LogInformation("Processing DebitRequestEvent [TransactionId={TransactionId}, UTR={UTR}, Retry={Retry}]", tx.TransactionId, tx.Utr, tx.RetryCount);
                await _debitService.HandleDebitRequest(tx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing DebitRequestEvent [TransactionId={TransactionId}, UTR={UTR}, Retry={Retry}]",
                    tx.TransactionId, tx.Utr, tx.RetryCount
                );
                var retryEvent = new DebitRequestEvent(
                    tx.TransactionId,
                    tx.Utr,
                    tx.SenderAccount,
                    tx.ReceiverAccount,
                    tx.Amount,
                    PaymentStatuses.DebitRequested,
                    DateTime.UtcNow,
                    tx.RetryCount + 1
                )
                {
                    CorrelationId = tx.CorrelationId,
                    TraceId = tx.TraceId               
                }; ;

                if (retryEvent.RetryCount <= 3)
                {
                    var delays = new[] { 1000, 5000, 15000 };
                    var delayMs = delays[Math.Min(retryEvent.RetryCount - 1, delays.Length - 1)];

                    _logger.LogWarning(
                        "Retrying DebitRequestEvent [TransactionId={TransactionId}, UTR={UTR}, Retry={Retry}] in {Delay}ms",
                        retryEvent.TransactionId, retryEvent.Utr, retryEvent.RetryCount, delayMs
                    );

                    await Task.Delay(delayMs);

                    await _debitProducer.ProduceDebitRetry(retryEvent);
                }
                else
                {
                    var failedEvent = new DebitFailedEvent(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, PaymentStatuses.DebitFailed, DateTime.UtcNow);
                    await _debitProducer.SendDebitRequestToDLQ(retryEvent);
                    await _debitProducer.ProduceDebitFailed(failedEvent);
                }

                throw;
            }
        }

        public Task Start(CancellationToken cancellationToken) =>
            new KafkaService<string, DebitRequestEvent>(
                "DebitRequestConsumer",
                QueueNames.Bank.DebitRequest,
                this,
                Deserializers.Utf8,
                new JsonDeserializer<DebitRequestEvent>()
            ).Run(cancellationToken);
    }
}