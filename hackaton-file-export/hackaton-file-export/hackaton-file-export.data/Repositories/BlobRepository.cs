

using hackaton_file_export.data.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace hackaton_file_export.data.Repositories
{
    public class BlobRepository : IBlobRepository
    {
        private readonly GridFSBucket _gridFS;

        public BlobRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("ASEHackathon");
            _gridFS = new GridFSBucket(database);
        }

        public async Task<byte[]> Get(ObjectId fileId)
        {
            return await _gridFS.DownloadAsBytesAsync(fileId);
        }
    }
}
