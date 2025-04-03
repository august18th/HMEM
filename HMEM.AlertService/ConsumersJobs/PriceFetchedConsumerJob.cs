using HMEM.MessageBroker;
using HMEM.MessageBroker.Models;
using Microsoft.Extensions.Hosting;
using HMEM.AlertService.Services;

namespace HMEM.AlertService.ConsumersJobs
{
    public class PriceFetchedConsumerJob : BackgroundService
    {
        private readonly ProcessorService _alertService;
        private readonly IKafkaConsumer _consumer;

        public PriceFetchedConsumerJob(ProcessorService alertService, IKafkaConsumerFactory consumerFactory)
        {
            _alertService = alertService;
            _consumer = consumerFactory.CreateConsumer("alert-service-group");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.Subscribe("alerts");

            await _consumer.ConsumeAsync<PriceFetchedMessage>(stoppingToken, async message =>
            {
                try
                {
                    await _alertService.Check(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing alert: {ex.Message}");
                }
            });
        }
    }
}
