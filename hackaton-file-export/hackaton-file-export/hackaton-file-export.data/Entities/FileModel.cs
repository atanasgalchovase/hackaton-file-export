using hackaton_file_export.common.Attributes;
using MongoDB.Bson;

namespace hackaton_file_export.data.Entities
{
    [BsonCollectionAttribute("files")]
    public class FileModel : BaseEntity
    {
        public ObjectId FileId { get; set; }

        public string FileName { get; set; }

        public string UserId { get; set; }
    }
}
