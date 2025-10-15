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

            // Log samo sa Console.WriteLine dok se ne popravi ServiceEventSource
            System.Console.WriteLine($"Uploading file: {file.FileName}");

            var filePath = _db.SaveFile(file.OpenReadStream(), file.FileName);

            var doc = new Document
            {
                FileName = file.FileName,
                FilePath = filePath
            };

            _db.SaveDocument(doc);

            System.Console.WriteLine($"File uploaded successfully: {file.FileName}");

            return Ok(new { Message = "File uploaded successfully", FileName = file.FileName });
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            System.Console.WriteLine("Getting all documents");

            var docs = _db.GetAllDocuments();

            System.Console.WriteLine($"Returning {docs.Count} documents");

            return Ok(docs);
        }

        // Dodajte health check endpoint
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Service = "DocumentService", Timestamp = System.DateTime.UtcNow });
        }
    }
}