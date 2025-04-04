using HMEM.Common.Models;
using MongoDB.Driver;

namespace HMEM.Data
{
    public class CryptoPriceRepository
    {
        private readonly IMongoCollection<PriceEntry> _priceCollection;

        public CryptoPriceRepository(MongoDbService mongoDbService)
        {
            _priceCollection = mongoDbService.GetPriceCollection();
        }

        public async Task SavePriceAsync(PriceEntry price)
        {
            await _priceCollection.InsertOneAsync(price);
        }

        public async Task<List<PriceEntry>> GetPricesAsync()
        {
            return await _priceCollection.Find(_ => true).ToListAsync();
        }

        public async Task<PriceEntry?> GetLatestPriceAsync(string symbol)
        {
            return await _priceCollection
                .Find(_ => _.Symbol == symbol)
                .SortByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();
        }
    }
}
