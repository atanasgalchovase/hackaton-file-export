using hackaton_file_export.common.Attributes;
using hackaton_file_export.data.Entities;
using hackaton_file_export.data.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace hackaton_file_export.data.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;

        public Repository(IOptions<MongoDbSettings> settings)
        {
            var database = new MongoClient(settings.Value.ConnectionString).GetDatabase(settings.Value.DatabaseName);
            _collection = database.GetCollection<T>(GetCollectionName(typeof(T)));
        }

        public async Task Add(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task<T> Get(ObjectId fileId)
        {
            var files = await _collection.FindAsync(c => c.Id == fileId);
            return files.FirstOrDefault();
        }

        private static string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                    typeof(BsonCollectionAttribute),
                    true)
                .FirstOrDefault())?.CollectionName;
        }
    }
}
