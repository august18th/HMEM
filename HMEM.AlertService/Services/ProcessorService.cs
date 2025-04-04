using HMEM.Common.Configuration;
using HMEM.Common.Models;
using HMEM.Data;
using HMEM.MessageBroker.Models;
using Telegram.Bot;

namespace HMEM.AlertService.Services;

public class ProcessorService
{
    private readonly CryptoPriceRepository _repository;
    private readonly TelegramBotClient _telegramBotClient;
    private readonly string _chatId;

    private readonly decimal _thresholdPercentage = 0.5m;

    public ProcessorService(CryptoPriceRepository repository, TelegramBotClient telegramBotClient, TelegramSettings settings)
    {
        _repository = repository;
        _telegramBotClient = telegramBotClient;
        _chatId = settings.ChatId;
    }

    public async Task ProcessNewPrice(PriceFetchedMessage message, CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine($"New message has arrived Symbol - {message.Symbol}, Price - {message.Price}");

            PriceEntry? previousPrice = await _repository.GetLatestPriceAsync(message.Symbol);

            await _repository.SavePriceAsync(new PriceEntry
            {
                Symbol = message.Symbol,
                Price = message.Price,
                Timestamp = message.Timestamp
            });

            if (previousPrice != null)
            {
                decimal percentageChange = Math.Abs((previousPrice.Price - message.Price) / previousPrice.Price * 100);

                if (percentageChange >= _thresholdPercentage)
                {
                    await _telegramBotClient.SendMessage(
                        chatId: _chatId,
                        text: $"🚨 Аномальна зміна ціни {message.Symbol}!\nЦіна: ${previousPrice.Price} → ${message.Price} ({percentageChange:F2}% за хвилину)",
                        cancellationToken: stoppingToken
                    );
                }
            }
            else
            {
                Console.WriteLine($"Previous value is null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AlertService: {ex.Message}");
        }
    }
}