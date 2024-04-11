using MongoDB.Bson;

namespace hackaton_file_export.data.Entities
{
    public abstract class BaseEntity
    {
        public ObjectId Id { get; set; }
    }
}
