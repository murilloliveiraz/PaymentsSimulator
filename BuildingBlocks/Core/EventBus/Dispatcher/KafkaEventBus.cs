using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Core.EventBus.Dispatcher;

public class KafkaEventBus : IEventBus, IEventBusProducer<string>, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

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
            GroupId = $"default-consumer-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }).Build();
    }

    public async Task PublishAsync(string topic, string key, string rawPayload, CancellationToken cancellationToken = default)
    {
        await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = rawPayload }, cancellationToken);
    }

    public void Subscribe<TEvent, THandler>(string topic)
    where TEvent : class
    where THandler : IEventHandler<TEvent>
    {
        Task.Run(() =>
        {
            var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = "127.0.0.1:9092",
                GroupId = $"consumer-{typeof(TEvent).Name}-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();

            consumer.Subscribe(topic);

            Console.WriteLine($"[KafkaEventBus] Subscribed to topic {topic} for event {typeof(TEvent).Name}");

            while (true)
            {
                try
                {
                    var result = consumer.Consume();

                    var message = JsonSerializer.Deserialize<TEvent>(result.Message.Value, _jsonOptions);

                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                    handler.HandleAsync(message!, CancellationToken.None).Wait(); // or make it async
                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EventBus] Error while consuming from {topic}: {ex.Message}");
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
