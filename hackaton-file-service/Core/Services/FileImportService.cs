using hackathon_file_import.Core.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace hackathon_file_import.Core.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly IRepository<byte[]> _repository;
        private readonly IConfiguration _configuration;
        public FileImportService(IRepository<byte[]> repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public bool IsValidFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var allowedFileTypes = _configuration.GetSection("AllowedFileTypes").Get<string[]>();
            if (!Array.Exists(allowedFileTypes, fileType => fileType.Equals(file.ContentType)))
            {
                return false;
            }

            return true;
            //var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            //return extension == ".csv" || extension == ".xlsx";
        }

        public void SaveFile(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                var fileBytes = memoryStream.ToArray();
                _repository.Add(fileBytes);
            }
        }
    }
}
