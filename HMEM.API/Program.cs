using HMEM.API.Services;
using HMEM.Data;
using HMEM.MessageBroker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["MongoDB:ConnectionString"];
    var databaseName = configuration["MongoDB:DatabaseName"];
    return new MongoDbService(connectionString, databaseName);
});

builder.Services.AddScoped<CryptoPriceRepository>();

builder.Services.AddHostedService<PriceFetcherService>();

builder.Services.AddKafkaProducer(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();