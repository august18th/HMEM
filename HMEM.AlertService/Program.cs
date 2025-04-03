using HMEM.AlertService.ConsumersJobs;
using HMEM.AlertService.Services;
using HMEM.Common.Configuration;
using HMEM.Data;
using HMEM.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var telegramConfig = context.Configuration.GetSection("Telegram").Get<TelegramSettings>();

        if (telegramConfig == null)
        {
            throw new ArgumentNullException(nameof(telegramConfig), "TelegramConfig is not found in configuration.");
        }

        services.AddSingleton(telegramConfig);

        var mongoConfig = context.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();

        if (mongoConfig == null)
        {
            throw new ArgumentNullException(nameof(mongoConfig), "MongoDB configuration is not found in appsettings.json.");
        }

        services.AddSingleton(mongoConfig);

        services.AddSingleton(sp =>
        {
            return new MongoDbService(mongoConfig.ConnectionString, mongoConfig.DatabaseName);
        });

        services.AddSingleton(sp =>
        {
            return new TelegramBotClient(telegramConfig.BotToken);
        });

        services.AddScoped<CryptoPriceRepository>();
        services.AddScoped<ProcessorService>();

        services.AddKafkaConsumer(context.Configuration);
        services.AddHostedService<PriceFetchedConsumerJob>();
    })
    .Build();

await host.RunAsync();