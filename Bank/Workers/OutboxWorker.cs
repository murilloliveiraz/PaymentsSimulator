using BuildingBlocks.Core.Interfaces;

namespace Bank.Workers
{
    class OutboxWorker : BackgroundService
    {
        private readonly ILogger<OutboxWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public OutboxWorker(ILogger<OutboxWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentsRepository>();

                    var producer = scope.ServiceProvider.GetRequiredService<IEventBusProducer<string>>();

                    var pending = await outboxRepository.GetPendingMessages();

                    foreach (var message in pending)
                    {
                        try
                        {
                            await producer.PublishAsync(message.Topic, message.CorrelationId, message.Payload, stoppingToken);

                            await outboxRepository.MarkAsPublishedAsync(message.MessageId);
                            await paymentRepository.UpdateStatus(message.CorrelationId, message.EventType);
                        }
                        catch (Exception ex)    
                        {
                            _logger.LogError(ex, "Falha ao publicar mensagem da outbox MessageId={MessageId}", message.MessageId);
                        }
                    }

                    // Delay antes de nova iteração
                    await Task.Delay(10000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado no OutboxWorker");
                    await Task.Delay(5000, stoppingToken); // backoff simples
                }
            }

            _logger.LogInformation("OutboxWorker finalizado.");
        }

    }
}
