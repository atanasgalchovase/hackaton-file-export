using hackaton_file_export.data.Entities;
using MongoDB.Bson;

namespace hackaton_file_export.data.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task Add(T entity);
        Task<T> Get(ObjectId fileId);
    }
}
