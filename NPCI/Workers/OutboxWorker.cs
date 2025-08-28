using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.Interfaces;
using NPCI.Repository.Interfaces;

namespace NPCI.Workers
{
    class OutboxWorker : BackgroundService
    {
        private readonly IOutboxRepository _outboxRepo;
        private readonly IPaymentsRepository _paymentRepository;
        private readonly IEventBusProducer<object> _eventBus;
        private readonly ILogger<OutboxWorker> _logger;

        public OutboxWorker(IOutboxRepository outboxRepo, ILogger<OutboxWorker> logger, IEventBusProducer<object> eventBus, IPaymentsRepository paymentRepository)
        {
            _outboxRepo = outboxRepo;
            _logger = logger;
            _eventBus = eventBus;
            _paymentRepository = paymentRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pending = await _outboxRepo.GetPendingMessages();

                    foreach (var message in pending)
                    {
                        try
                        {
                            await _eventBus.PublishAsync(message.Topic, message.CorrelationId, message.Payload, stoppingToken);

                            await _outboxRepo.MarkAsPublishedAsync(message.MessageId);
                            await _paymentRepository.UpdateStatus(message.CorrelationId, message.EventType);
                        }
                        catch (Exception ex)    
                        {
                            _logger.LogError(ex, "Falha ao publicar mensagem da outbox MessageId={MessageId}", message.MessageId);
                        }
                    }

                    // Delay antes de nova iteração
                    await Task.Delay(1000, stoppingToken);
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
