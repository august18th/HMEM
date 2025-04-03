using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMEM.MessageBroker
{
    public static class MessageBrokerExtensions
    {
        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection producerConfig = configuration.GetSection("Kafka:ProducerConfig");

            services.Configure<ProducerConfig>(producerConfig.Bind);

            services.AddSingleton<IKafkaProducer, KafkaProducer>();

            return services;
        }

        public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection consumerConfig = configuration.GetSection("Kafka:ConsumerConfig");

            services.Configure<ConsumerConfig>(consumerConfig.Bind);

            services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();

            return services;
        }
    }
}
