using HMEM.Common.Models;
using HMEM.Data;
using System.Text.Json;

namespace HMEM.API
{
    public class PriceFetcherService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceFetcherService> _logger;

        public PriceFetcherService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, ILogger<PriceFetcherService> logger)
        {
            _serviceProvider = serviceProvider;
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
                    _logger.LogInformation("Fetching Ethereum price at: {time}", DateTime.UtcNow);

                    var price = await FetchPriceAsync();
                    if (price != null)
                    {
                        // Отримуємо scoped сервіс із провайдера
                        using var scope = _serviceProvider.CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<CryptoPriceRepository>();

                        var entry = new PriceEntry
                        {
                            Price = price.Value,
                            Timestamp = DateTime.UtcNow
                        };
                        await repository.SavePriceAsync(entry);

                        _logger.LogInformation("Successfully fetched and saved price: {price} USD at {time}", price, entry.Timestamp);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to fetch price. The response was null.");
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

        private async Task<decimal?> FetchPriceAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://api.coingecko.com/api/v3/simple/price?ids=ethereum&vs_currencies=usd");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);

                    if (data.TryGetProperty("ethereum", out var ethData) &&
                        ethData.TryGetProperty("usd", out var price))
                    {
                        return price.GetDecimal();
                    }
                    else
                    {
                        _logger.LogWarning("Unexpected response format from API.");
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

            return null;
        }
    }

}