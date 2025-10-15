using Microsoft.AspNetCore.Mvc;
using ChatService.Models;
using ChatService.Services;
using System;
using System.Threading.Tasks;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatDatabaseService _db;

        public ChatController(ChatDatabaseService db)
        {
            _db = db;
        }

        [HttpPost("ask")]
        public IActionResult Ask([FromBody] ChatQuestionRequest request)
        {
            if (string.IsNullOrEmpty(request?.Question))
                return BadRequest("Question is required");

            // Ovde ide integracija sa LLM kasnije
            var answer = $"Ovo je odgovor na: {request.Question}";

            var message = new ChatMessage
            {
                Question = request.Question,
                Answer = answer,
                CreatedAt = DateTime.UtcNow
            };

            _db.SaveMessage(message);

            return Ok(new { Answer = answer });
        }

        [HttpGet("history")]
        public IActionResult History()
        {
            var messages = _db.GetAllMessages();
            return Ok(messages);
        }
    }

    public class ChatQuestionRequest
    {
        public string Question { get; set; }
    }
}