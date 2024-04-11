using hackaton_file_export.common.Attributes;
using hackaton_file_export.services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace hackaton_file_export.api.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileImportController : ControllerBase
    {

        private readonly IFileImportService _fileImportService;

        public FileImportController(IFileImportService fileImportService)
        {
            _fileImportService = fileImportService;
        }

        [HttpPost("download")]
        [AuthorizeAttribute(Roles: new string[] { "ReadOnly", "User" })]
        public async Task<IActionResult> DownloadFile(string id)
        {
            var file = await _fileImportService.DownloadFile(ObjectId.Parse(id));
            return Ok(file);
        }
    }
}
