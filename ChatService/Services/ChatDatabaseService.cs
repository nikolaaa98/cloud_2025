using Microsoft.Data.Sqlite;
using ChatService.Models;
using System.Collections.Generic;
using System.IO;

namespace ChatService.Services
{
    public class ChatDatabaseService
    {
        private readonly string _dbPath;

        public ChatDatabaseService()
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "chat.db");

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS ChatMessages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Question TEXT NOT NULL,
                Answer TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            ";
            tableCmd.ExecuteNonQuery();
        }

        public void SaveMessage(ChatMessage message)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText =
            @"
            INSERT INTO ChatMessages (Question, Answer, CreatedAt)
            VALUES ($question, $answer, $createdAt);
            ";
            insertCmd.Parameters.AddWithValue("$question", message.Question);
            insertCmd.Parameters.AddWithValue("$answer", message.Answer);
            insertCmd.Parameters.AddWithValue("$createdAt", message.CreatedAt.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }

        public List<ChatMessage> GetAllMessages()
        {
            var messages = new List<ChatMessage>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id, Question, Answer, CreatedAt FROM ChatMessages ORDER BY CreatedAt DESC;";

            using var reader = selectCmd.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new ChatMessage
                {
                    Id = reader.GetInt32(0),
                    Question = reader.GetString(1),
                    Answer = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3))
                });
            }

            return messages;
        }
    }
}
