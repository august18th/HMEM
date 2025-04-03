using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HMEM.MessageBroker
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, T message) where T : class;
        Task ProduceAsync<T>(string topic, string key, T message) where T : class;
    }

    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IOptions<ProducerConfig> config)
        {
            _producer = new ProducerBuilder<string, string>(config.Value)
                .SetErrorHandler((_, e) => Console.WriteLine($"Kafka error: {e.Reason}"))
                .Build();
        }

        public async Task ProduceAsync<T>(string topic, T message) where T : class
        {
            await ProduceAsync(topic, Guid.NewGuid().ToString(), message);
        }

        public async Task ProduceAsync<T>(string topic, string key, T message) where T : class
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            string serializedMessage = JsonSerializer.Serialize(message);

            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = serializedMessage
            };

            try
            {
                var result = await _producer.ProduceAsync(topic, kafkaMessage);
                Console.WriteLine($"Message delivered to: {result.TopicPartitionOffset}");
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"Failed to deliver message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
