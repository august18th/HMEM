using HMEM.Common.Models;
using MongoDB.Driver;

namespace HMEM.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<PriceEntry> GetPriceCollection()
        {
            return _database.GetCollection<PriceEntry>("Prices");
        }
    }
}
