using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HMEM.MessageBroker
{
    public interface IKafkaConsumer
    {
        Task Subscribe(string topic);
        Task ConsumeAsync<T>(CancellationToken cancellationToken, Func<T, Task> messageHandler) where T : class;
    }

    public interface IKafkaConsumerFactory
    {
        IKafkaConsumer CreateConsumer(string groupId);
    }

    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly IOptions<ConsumerConfig> _baseConfig;

        public KafkaConsumerFactory(IOptions<ConsumerConfig> baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public IKafkaConsumer CreateConsumer(string groupId)
        {
            var config = new ConsumerConfig(_baseConfig.Value)
            {
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            return new KafkaConsumer(config);
        }
    }

    public class KafkaConsumer : IKafkaConsumer, IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private string _topic;

        public KafkaConsumer(ConsumerConfig config)
        {
            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => Console.WriteLine($"Kafka error: {e.Reason}"))
                .Build();
        }

        public Task Subscribe(string topic)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic));

            _topic = topic;
            _consumer.Subscribe(topic);
            return Task.CompletedTask;
        }

        public async Task ConsumeAsync<T>(CancellationToken cancellationToken, Func<T, Task> messageHandler) where T : class
        {
            if (string.IsNullOrEmpty(_topic))
                throw new InvalidOperationException("Must subscribe to a topic before consuming");

            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);

                        if (consumeResult == null || consumeResult.Message == null)
                            continue;

                        try
                        {
                            var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                            if (message != null)
                            {
                                await messageHandler(message);
                            }
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Failed to deserialize message: {ex.Message}");
                        }

                        // Commit the offset
                        _consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Consumer error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was canceled, consumer is being closed
                _consumer.Close();
            }
        }

        public void Dispose()
        {
            _consumer?.Dispose();
        }
    }
}
