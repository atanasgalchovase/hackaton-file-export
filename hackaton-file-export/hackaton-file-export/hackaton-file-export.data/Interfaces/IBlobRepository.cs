using MongoDB.Bson;

namespace hackaton_file_export.data.Interfaces
{
    public interface IBlobRepository
    {
        Task<byte[]> Get(ObjectId fileId);
    }
}
