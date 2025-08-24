namespace BuildingBlocks.Core.Services
{

    public class KafkaService<TKey, TValue> : IDisposable
    {
        private readonly IConsumer<TKey, TValue> _consumer;
        private readonly IConsumerFunction<TKey, TValue> _parseFunction;
        private readonly string _groupId;
        private readonly string _topic;

        public KafkaService(string groupId, string topic, IConsumerFunction<TKey, TValue> parseFunction, IDeserializer<TKey> keyDeserializer, IDeserializer<TValue> valueDeserializer)
        {
            _groupId = groupId;
            _topic = topic;
            _parseFunction = parseFunction;

            var config = GetConsumerConfig(groupId);

            _consumer = new ConsumerBuilder<TKey, TValue>(config)
                .SetKeyDeserializer(keyDeserializer)
                .SetValueDeserializer(valueDeserializer)
                .SetErrorHandler((_, e) => Console.WriteLine($"KafkaService ({groupId}): Erro interno do Kafka: {e.Reason}"))
                .SetStatisticsHandler((_, json) => Console.WriteLine($"KafkaService ({groupId}): Estatísticas Kafka: {json}"))
                .Build();

            if (topic.Contains(".*") || topic.Contains("^"))
            {
                _consumer.Subscribe('^' + topic);
            }
            else
            {
                _consumer.Subscribe(topic);
            }

            Console.WriteLine($"KafkaService ({groupId}): Inscrito no tópico: {topic}");
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);

                        if (consumeResult == null)
                        {
                            continue;
                        }

                        _parseFunction.Consume(consumeResult);

                        _consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"KafkaService ({_groupId}): Erro ao consumir mensagem: {ex.Error.Reason}");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"KafkaService ({_groupId}): Consumo cancelado.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"KafkaService ({_groupId}): Erro inesperado durante o processamento: {ex.Message}");
                    }
                }
            }
            finally
            {
                Console.WriteLine($"KafkaService ({_groupId}): Fechando consumidor...");
                _consumer.Close();
            }
        }

        private static ConsumerConfig GetConsumerConfig(string groupId)
        {
            return new ConsumerConfig
            {
                BootstrapServers = "127.0.0.1:9092",
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            };
        }

        public void Dispose()
        {
            _consumer?.Dispose();
        }
    }
}
