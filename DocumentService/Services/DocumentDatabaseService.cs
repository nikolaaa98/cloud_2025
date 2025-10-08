using Microsoft.Data.Sqlite;
using DocumentService.Models;
using System.Collections.Generic;
using System.IO;

namespace DocumentService.Services
{
    public class DocumentDatabaseService
    {
        private readonly string _dbPath;
        private readonly string _fileFolder;

        public DocumentDatabaseService()
        {
            _fileFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Files");
            Directory.CreateDirectory(_fileFolder);

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "documents.db");

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Documents (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                UploadedAt TEXT NOT NULL
            );
            ";
            tableCmd.ExecuteNonQuery();
        }

        public string SaveFile(Stream fileStream, string fileName)
        {
            var path = Path.Combine(_fileFolder, fileName);
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write);
            fileStream.CopyTo(file);
            return path;
        }

        public void SaveDocument(Document doc)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText =
            @"
            INSERT INTO Documents (FileName, FilePath, UploadedAt)
            VALUES ($fileName, $filePath, $uploadedAt);
            ";
            insertCmd.Parameters.AddWithValue("$fileName", doc.FileName);
            insertCmd.Parameters.AddWithValue("$filePath", doc.FilePath);
            insertCmd.Parameters.AddWithValue("$uploadedAt", doc.UploadedAt.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }

        public List<Document> GetAllDocuments()
        {
            var docs = new List<Document>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id, FileName, FilePath, UploadedAt FROM Documents ORDER BY UploadedAt DESC;";

            using var reader = selectCmd.ExecuteReader();
            while (reader.Read())
            {
                docs.Add(new Document
                {
                    Id = reader.GetInt32(0),
                    FileName = reader.GetString(1),
                    FilePath = reader.GetString(2),
                    UploadedAt = DateTime.Parse(reader.GetString(3))
                });
            }

            return docs;
        }
    }
}
