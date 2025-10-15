using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly string _uploadPath;
        private readonly string _dbPath;

        public DocumentController()
        {
            // Inicijalizuj SQLite
            Batteries.Init();

            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "TempUploads");
            Directory.CreateDirectory(_uploadPath);

            var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            Directory.CreateDirectory(dataPath);
            _dbPath = Path.Combine(dataPath, "gateway_documents.db");

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Documents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    UploadedAt TEXT NOT NULL
                );";
            tableCmd.ExecuteNonQuery();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(_uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                SaveDocumentToLocalDb(file.FileName, filePath);

                return Ok(new
                {
                    Message = "✅ File uploaded successfully!",
                    FileName = file.FileName,
                    UploadedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "❌ Upload failed",
                    Error = ex.Message
                });
            }
        }

        private void SaveDocumentToLocalDb(string fileName, string filePath)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Documents (FileName, FilePath, UploadedAt)
                VALUES ($fileName, $filePath, $uploadedAt);";
            insertCmd.Parameters.AddWithValue("$fileName", fileName);
            insertCmd.Parameters.AddWithValue("$filePath", filePath);
            insertCmd.Parameters.AddWithValue("$uploadedAt", DateTime.UtcNow.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            try
            {
                var docs = GetDocumentsFromLocalDb();
                return Ok(docs);
            }
            catch (Exception)
            {
                return Ok(new List<object>());
            }
        }

        private List<object> GetDocumentsFromLocalDb()
        {
            var docs = new List<object>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id, FileName, FilePath, UploadedAt FROM Documents ORDER BY UploadedAt DESC;";

            using var reader = selectCmd.ExecuteReader();
            while (reader.Read())
            {
                docs.Add(new
                {
                    Id = reader.GetInt32(0),
                    FileName = reader.GetString(1),
                    UploadedAt = reader.GetString(3)
                    //UploadedAt = DateTime.Parse(reader.GetString(3)).ToString("yyyy-MM-ddTHH:mm:ssZ")
                });
            }

            return docs;
        }

        [HttpGet("debug")]
        public IActionResult DebugInfo()
        {
            Batteries.Init();

            var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            var dbPath = Path.Combine(dataPath, "gateway_documents.db");

            var dbExists = System.IO.File.Exists(dbPath);
            var documentCount = 0;
            var tempUploadsExists = Directory.Exists(_uploadPath);
            var tempFileCount = 0;

            if (tempUploadsExists)
            {
                tempFileCount = Directory.GetFiles(_uploadPath).Length;
            }

            if (dbExists)
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                var countCmd = connection.CreateCommand();
                countCmd.CommandText = "SELECT COUNT(*) FROM Documents;";
                documentCount = Convert.ToInt32(countCmd.ExecuteScalar());
            }

            return Ok(new
            {
                DatabaseExists = dbExists,
                DatabasePath = dbPath,
                DocumentCount = documentCount,
                DataDirectoryExists = Directory.Exists(dataPath),
                TempUploadsExists = tempUploadsExists,
                TempFileCount = tempFileCount,
                CurrentDirectory = Directory.GetCurrentDirectory()
            });
        }
    }
}