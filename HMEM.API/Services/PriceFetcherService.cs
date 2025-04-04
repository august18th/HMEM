using HMEM.Common.Models.StockModels;
using HMEM.MessageBroker;
using HMEM.MessageBroker.Models;
using System.Text.Json;

namespace HMEM.API.Services
{
    public class PriceFetcherService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PriceFetcherService> _logger;
        private readonly IKafkaProducer _kafkaProducer;

        private readonly List<string> currencies = new List<string>()
        {
            "BTCUSDT",
            "ETHUSDT"
        };

        public PriceFetcherService(
            IKafkaProducer kafkaProducer,
            IHttpClientFactory httpClientFactory,
            ILogger<PriceFetcherService> logger)
        {
            _kafkaProducer = kafkaProducer;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PriceFetcherService started at: {time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<BinancePrice> binancePrices = await FetchPriceAsync();

                    foreach (var priceModel in binancePrices)
                    {
                        _logger.LogInformation($"Message with {priceModel.Symbol} sent");

                        PriceFetchedMessage message = new PriceFetchedMessage
                        {
                            Price = decimal.Parse(priceModel.Price),
                            Timestamp = DateTime.UtcNow,
                            Symbol = priceModel.Symbol
                        };

                        await _kafkaProducer.ProduceAsync("alerts", message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching Ethereum price");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("PriceFetcherService is stopping at: {time}", DateTime.UtcNow);
        }

        private async Task<List<BinancePrice>> FetchPriceAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://api.binance.com/api/v3/ticker/price");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<BinancePrice>>(json);

                    if (data != null)
                    {
                        var filtered = data
                            .Where(p => currencies.Contains(p.Symbol))
                            .ToList();

                        return filtered;
                    }
                    else
                    {
                        _logger.LogWarning("data is null");
                    }
                }
                else
                {
                    _logger.LogWarning("API call failed with status code: {statusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during API call.");
            }

            return new List<BinancePrice>();
        }
    }

}