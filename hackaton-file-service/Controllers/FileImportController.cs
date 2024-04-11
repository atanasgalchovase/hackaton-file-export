using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using hackathon_file_import.Core.Interfaces;
using hackathon_file_import.Core.Services;
using MongoDB.Bson;
using System.Collections.Generic;
using hackathon_file_import.Infrastructure.Configurations;
using Newtonsoft.Json;
using SharpCompress.Common;
using Microsoft.Extensions.Configuration;

namespace hackathon_file_import.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileImportController : ControllerBase
    {

        private readonly IFileImportService _fileImportService;
        private readonly FileUploadMessageResults _fileUploadResults;
        private readonly string _invalidFileTypeMessage;
        private readonly string _successMessage;
        public FileImportController(IFileImportService fileImportService, IConfiguration configuration)
        {
            _fileImportService = fileImportService;
            _fileUploadResults = configuration.GetSection("FileUploadResults").Get<FileUploadMessageResults>();
            _invalidFileTypeMessage = _fileUploadResults.InvalidFileTypeMessage;
            _successMessage = _fileUploadResults.SuccessMessage;
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (!_fileImportService.IsValidFile(file))
            {
                return BadRequest(_invalidFileTypeMessage);
            }

            _fileImportService.SaveFile(file);
            return Ok(_successMessage);
        }
    }
}
