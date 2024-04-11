using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace hackaton_file_export.services.Interfaces
{
    public interface IFileImportService
    {
        Task<byte[]> DownloadFile(ObjectId id);
    }
}
