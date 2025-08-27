namespace BuildingBlocks.Core.EventBus.Dispatcher;

public class KafkaDispatcher<TKey, TValue> : IDisposable
{
    private readonly IProducer<TKey, TValue> _producer;

    public KafkaDispatcher(ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "127.0.0.1:9092",
            Acks = Acks.All,
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<TKey, TValue>(producerConfig)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .SetErrorHandler((_, e) => Console.WriteLine($"Erro interno do Kafka no Dispatcher: {e.Reason}"))
            .Build();
    }

    public async Task SendAsync(string topic, TKey key, TValue value)
    {
        var message = new Message<TKey, TValue>
        {
            Key = key,
            Value = value
        };

        try
        {
            Console.WriteLine($"Dispatcher: Tentando enviar mensagem para o tópico {topic}...");
            var deliveryResult = await _producer.ProduceAsync(topic, message);

            if (deliveryResult.Status == PersistenceStatus.Persisted)
            {
                Console.WriteLine($"Dispatcher: Sucesso enviando para {deliveryResult.Topic} | Partição {deliveryResult.Partition.Value} | Offset {deliveryResult.Offset.Value} | Timestamp {deliveryResult.Timestamp.UtcDateTime}");
            }
            else
            {
                Console.WriteLine($"Dispatcher: Mensagem não persistida para {deliveryResult.Topic}: Status {deliveryResult.Status}");
            }
        }
        catch (ProduceException<TKey, TValue> ex)
        {
            Console.WriteLine($"Dispatcher: Erro ao produzir mensagem para {topic}: {ex.Error.Reason}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dispatcher: Erro inesperado ao enviar mensagem para {topic}: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        Console.WriteLine("Dispatcher: Fechando produtor...");
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}