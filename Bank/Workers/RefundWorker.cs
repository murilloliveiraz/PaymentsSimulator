using Bank.Repository.Interfaces;
using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using System.Text.Json;

namespace Bank.Workers
{
    class RefundWorker : BackgroundService
    {
        private readonly ILogger<RefundWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RefundWorker(ILogger<RefundWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RefundWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var refundRepository = scope.ServiceProvider.GetRequiredService<IRefundRepository>();
                    var refundService = scope.ServiceProvider.GetRequiredService<RefundService>();
                    var producer = scope.ServiceProvider.GetRequiredService<IEventBusProducer<string>>();

                    var pendingRefunds = await refundRepository.GetAllPendingRefunds();

                    foreach (var refund in pendingRefunds)
                    {
                        try
                        {
                            var result = await refundService.HandleRefund(refund);

                            if (!result.IsSuccess)
                            {
                                await refundRepository.IncrementRetry(refund.Utr, result.ErrorMessage);

                                if (refund.RetryCount + 1 >= 3)
                                {
                                    await refundRepository.MarkAsDlq(refund.Utr, result.ErrorMessage);
                                    _logger.LogWarning("Refund DLQ para UTR={Utr}", refund.Utr);

                                    await outboxRepository.AddAsync(new OutboxMessage
                                    {
                                        CorrelationId = refund.Utr,
                                        Topic = QueueNames.Bank.RefundRequest + ".DLQ",
                                        EventType = nameof(RefundRequestEvent),
                                        Payload = JsonSerializer.Serialize(refund),
                                        Status = OutboxStatus.Pending,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                }

                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro inesperado processando refund UTR={Utr}", refund.Utr);

                            await refundRepository.IncrementRetry(refund.Utr, ex.Message);

                            if (refund.RetryCount + 1 >= 3)
                            {
                                await refundRepository.MarkAsDlq(refund.Utr, ex.Message);
                                _logger.LogWarning("Refund DLQ para UTR={Utr}", refund.Utr);

                                await outboxRepository.AddAsync(new OutboxMessage
                                {
                                    CorrelationId = refund.Utr,
                                    Topic = QueueNames.Bank.RefundRequest + ".DLQ",
                                    EventType = nameof(RefundRequestEvent),
                                    Payload = JsonSerializer.Serialize(refund),
                                    Status = OutboxStatus.Pending,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    await Task.Delay(10000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado no RefundWorker");
                    await Task.Delay(5000, stoppingToken); // backoff simples
                }
            }

            _logger.LogInformation("RefundWorker finalizado.");
        }

    }
}
