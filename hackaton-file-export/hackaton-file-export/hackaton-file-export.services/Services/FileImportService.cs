using hackaton_file_export.data.Entities;
using hackaton_file_export.data.Interfaces;
using hackaton_file_export.services.Interfaces;
using MongoDB.Bson;

namespace hackaton_file_export.services
{
    public class FileImportService : IFileImportService
    {
        private readonly IBlobRepository _blobRepository;
        private readonly IRepository<FileModel> _fileRepository;

        public FileImportService(IBlobRepository repository, IRepository<FileModel> fileRepository)
        {
            _blobRepository = repository;
            _fileRepository = fileRepository;
        }

        public async Task<byte[]> DownloadFile(ObjectId fileId)
        {
           var file = await _blobRepository.Get(fileId);

            return file;
        }
    }
}
