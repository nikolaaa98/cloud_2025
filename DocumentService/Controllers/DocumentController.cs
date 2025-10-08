using Microsoft.AspNetCore.Mvc;
using DocumentService.Services;
using DocumentService.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DocumentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentDatabaseService _db;

        public DocumentController(DocumentDatabaseService db)
        {
            _db = db;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = _db.SaveFile(file.OpenReadStream(), file.FileName);

            var doc = new Document
            {
                FileName = file.FileName,
                FilePath = filePath
            };

            _db.SaveDocument(doc);

            return Ok(new { Message = "File uploaded successfully", FileName = file.FileName });
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var docs = _db.GetAllDocuments();
            return Ok(docs);
        }
    }
}
