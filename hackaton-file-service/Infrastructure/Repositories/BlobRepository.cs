using hackathon_file_import.Core.Interfaces;
using hackathon_file_import.Infrastructure.DataAccess;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using MongoDB.Driver;

namespace hackathon_file_import.Infrastructure.Repositories
{
    public class BlobRepository : IRepository<byte[]>
    {
        private readonly GridFSBucket _gridFS;

        public BlobRepository(MongoDbContext dbContext)
        {
            _gridFS = new GridFSBucket(dbContext.Database, new GridFSBucketOptions
            {
                BucketName = "fs",
                ChunkSizeBytes = 1048576, // 1MB chunk size
                DisableMD5 = false,
                WriteConcern = WriteConcern.WMajority
            });

            
        }

        public void Add(byte[] entity)
        {
            var metadata = new BsonDocument
            {
                { "userid", new BsonString("admin") }, // Placeholder for userid
                { "extension", new BsonString("csv") }, // Placeholder for file extension
                { "isdeleted", new BsonBoolean(true) }, // Placeholder for deletion flag
                { "deletedby", new BsonString("admin") }, // Placeholder for deleted by
                { "deleteddate", new BsonDateTime(DateTime.UtcNow) } // Placeholder for deletion date
            };


            _gridFS.UploadFromBytes("files", entity, new GridFSUploadOptions
            {
                Metadata = metadata
            });
        }
    }
}
