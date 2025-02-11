using HMEM.Common.Configuration;
using HMEM.Data;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace HMEM.Notification;

public class AlertService : BackgroundService
{
    private readonly CryptoPriceRepository _repository;
    private readonly TelegramBotClient _telegramBotClient;
    private readonly string _chatId;

    public AlertService(CryptoPriceRepository repository, TelegramBotClient telegramBotClient, TelegramSettings settings)
    {
        _repository = repository;
        _telegramBotClient = telegramBotClient;
        _chatId = settings.ChatId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        decimal? previousPrice = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var latestPrice = await _repository.GetLatestPriceAsync();

                if (previousPrice.HasValue && latestPrice != null)
                {
                    decimal oldHundreds = Math.Floor(previousPrice.Value / 100) * 100;
                    decimal newHundreds = Math.Floor(latestPrice.Price / 100) * 100;

                    if (oldHundreds != newHundreds)
                    {
                        await _telegramBotClient.SendMessage(
                            chatId: _chatId,
                            text: $"Ethereum price changed by $100 or more! New price: ${latestPrice.Price}",
                            cancellationToken: stoppingToken
                        );
                    }
                }

                previousPrice = latestPrice?.Price;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AlertService: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
