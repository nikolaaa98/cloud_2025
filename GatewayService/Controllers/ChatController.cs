using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System;
using SQLitePCL;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly string _dbPath;
        private readonly HttpClient _httpClient;
        private readonly string _aiServiceUrl = "http://localhost:5070"; // Tvoj LLM servis

        public ChatController(IHttpClientFactory httpClientFactory)
        {
            Batteries.Init();
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);

            var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            Directory.CreateDirectory(dataPath);
            _dbPath = Path.Combine(dataPath, "gateway_chat.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ChatMessages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Question TEXT NOT NULL,
                    Answer TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    Source TEXT NOT NULL DEFAULT 'AI'
                );";
            tableCmd.ExecuteNonQuery();
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Question))
                return BadRequest("Question is required");

            string answer;
            string source = "AI";

            try
            {
                // Pokušaj da pozoveš AI servis
                answer = await GetAIResponse(request.Question);
                source = "AI";
            }
            catch (Exception ex)
            {
                // Fallback na običan odgovor ako AI ne radi
                answer = $"I received your question: '{request.Question}'. " +
                        $"AI service response: {ex.Message}";
                source = "Fallback";
            }

            // Sačuvaj poruku u bazu
            SaveMessage(request.Question, answer, source);

            return Ok(new
            {
                Answer = answer,
                Source = source,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });
        }

        private async Task<string> GetAIResponse(string question)
        {
            try
            {
                var requestData = new { prompt = question };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine($"🤖 [DEBUG] Calling AI: {_aiServiceUrl}/chat");
                Console.WriteLine($"🤖 [DEBUG] Sending: {json}");

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/chat", content);

                Console.WriteLine($"🤖 [DEBUG] AI Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"🤖 [DEBUG] AI Raw Response: {responseJson}");

                    // POKUŠAJ RAZLIČITE JSON FORMATE

                    // 1. Prvo probaj sa "response" poljem (tvoj LLM format)
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var aiResponse = JsonSerializer.Deserialize<AILegacyResponse>(responseJson, options);
                        if (!string.IsNullOrEmpty(aiResponse?.response))
                        {
                            Console.WriteLine($"🤖 [DEBUG] Using 'response' field: {aiResponse.response}");
                            return aiResponse.response.Trim();
                        }
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine($"🤖 [DEBUG] Failed to parse 'response': {ex1.Message}");
                    }

                    // 2. Probaj sa "Response" poljem (stari format)
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var aiResponse = JsonSerializer.Deserialize<AIResponse>(responseJson, options);
                        if (!string.IsNullOrEmpty(aiResponse?.Response))
                        {
                            Console.WriteLine($"🤖 [DEBUG] Using 'Response' field: {aiResponse.Response}");
                            return aiResponse.Response.Trim();
                        }
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"🤖 [DEBUG] Failed to parse 'Response': {ex2.Message}");
                    }

                    // 3. Probaj sa "content" poljem (drugi format)
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var aiResponse = JsonSerializer.Deserialize<AIContentResponse>(responseJson, options);
                        if (!string.IsNullOrEmpty(aiResponse?.content))
                        {
                            Console.WriteLine($"🤖 [DEBUG] Using 'content' field: {aiResponse.content}");
                            return aiResponse.content.Trim();
                        }
                    }
                    catch (Exception ex3)
                    {
                        Console.WriteLine($"🤖 [DEBUG] Failed to parse 'content': {ex3.Message}");
                    }

                    // 4. Ako je običan string, vrati ga
                    if (!string.IsNullOrEmpty(responseJson) && responseJson.Length < 500)
                    {
                        Console.WriteLine($"🤖 [DEBUG] Using raw response as string");
                        return responseJson.Trim();
                    }

                    // 5. Ako ništa ne uspe, vrati fallback
                    Console.WriteLine($"🤖 [DEBUG] All parsing failed, using fallback");
                    return $"AI processing: {question}";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"🤖 [DEBUG] AI Error: {errorContent}");
                    throw new Exception($"AI service returned: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🤖 [DEBUG] AI Communication Failed: {ex.Message}");
                throw new Exception($"AI communication failed: {ex.Message}");
            }
        }

        [HttpGet("history")]
        public IActionResult History()
        {
            var messages = GetChatHistory();
            return Ok(messages);
        }

        private void SaveMessage(string question, string answer, string source)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO ChatMessages (Question, Answer, CreatedAt, Source)
                VALUES ($question, $answer, $createdAt, $source);";
            insertCmd.Parameters.AddWithValue("$question", question);
            insertCmd.Parameters.AddWithValue("$answer", answer);
            insertCmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
            insertCmd.Parameters.AddWithValue("$source", source);
            insertCmd.ExecuteNonQuery();
        }

        private List<object> GetChatHistory()
        {
            var messages = new List<object>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id, Question, Answer, CreatedAt, Source FROM ChatMessages ORDER BY CreatedAt DESC;";

            using var reader = selectCmd.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new
                {
                    Id = reader.GetInt32(0),
                    Question = reader.GetString(1),
                    Answer = reader.GetString(2),
                    CreatedAt = reader.GetString(3), // Ostavi kao string
                    Source = reader.GetString(4)
                });
            }

            return messages;
        }

        [HttpGet("test-db")]
        public IActionResult TestDb()
        {
            try
            {
                var messages = GetChatHistory();
                return Ok(new
                {
                    MessageCount = messages.Count,
                    Messages = messages,
                    DatabasePath = _dbPath,
                    DatabaseExists = System.IO.File.Exists(_dbPath)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("debug")]
        public IActionResult DebugInfo()
        {
            var dbExists = System.IO.File.Exists(_dbPath);
            var messageCount = 0;

            if (dbExists)
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var countCmd = connection.CreateCommand();
                countCmd.CommandText = "SELECT COUNT(*) FROM ChatMessages;";
                messageCount = Convert.ToInt32(countCmd.ExecuteScalar());
            }

            return Ok(new
            {
                ChatDatabaseExists = dbExists,
                ChatDatabasePath = _dbPath,
                MessageCount = messageCount,
                AIServiceUrl = _aiServiceUrl,
                AIServiceStatus = "Configured"
            });
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; }
    }

    // RAZLIČITI JSON FORMATI KOJE AI MOŽE VRATITI
    public class AILegacyResponse
    {
        public string response { get; set; }  // mala slova - tvoj LLM format
    }

    public class AIResponse
    {
        public string Response { get; set; }  // velika slova - stari format
    }

    public class AIContentResponse
    {
        public string content { get; set; }   // content polje
    }
}