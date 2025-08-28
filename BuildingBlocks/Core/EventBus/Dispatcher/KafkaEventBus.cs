using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Core.EventBus.Dispatcher;

public class KafkaEventBus : IEventBus, IEventBusProducer<object>, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;

    public KafkaEventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = "127.0.0.1:9092"
        }).Build();

        _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = "127.0.0.1:9092",
            GroupId = "default-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }).Build();
    }

    public async Task PublishAsync(string topic, string key, object @event, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(@event);
        await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = payload }, cancellationToken);
    }

    public void Subscribe<TEvent, THandler>(string topic)
        where TEvent : class
        where THandler : IEventHandler<TEvent>
    {
        _consumer.Subscribe(topic);

        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var result = _consumer.Consume();

                    var message = JsonSerializer.Deserialize<TEvent>(result.Message.Value);

                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                    await ExecuteWithRetry(
                        async () => await handler.HandleAsync(message!, default),
                        result, topic);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EventBus] Erro inesperado: {ex.Message}");
                }
            }
        });
    }

    private async Task ExecuteWithRetry(Func<Task> action, ConsumeResult<string, string> result, string topic)
    {
        int maxRetries = 3;
        int delay = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await action();
                _consumer.Commit(result);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Retry] Tentativa {attempt}/{maxRetries} falhou: {ex.Message}");
                await Task.Delay(delay);
                delay *= 2;
            }
        }

        // fallback DLQ
        var dlqTopic = $"{topic}.DLQ";
        await _producer.ProduceAsync(dlqTopic, new Message<string, string>
        {
            Key = result.Message.Key,
            Value = result.Message.Value
        });

        Console.WriteLine($"[DLQ] Mensagem enviada para {dlqTopic}");
    }

    public void Dispose()
    {
        _producer?.Dispose();
        _consumer?.Dispose();
    }
}
