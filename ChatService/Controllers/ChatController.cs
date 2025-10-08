using Microsoft.AspNetCore.Mvc;
using ChatService.Models;
using ChatService.Services;
using System;

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
        public IActionResult Ask([FromBody] string question)
        {
            // Ovde ide integracija sa LLM kasnije
            var answer = $"Ovo je odgovor na: {question}";

            var message = new ChatMessage
            {
                Question = question,
                Answer = answer,
                CreatedAt = DateTime.UtcNow
            };

            _db.SaveMessage(message);

            return Ok(answer);
        }

        [HttpGet("history")]
        public IActionResult History()
        {
            var messages = _db.GetAllMessages();
            return Ok(messages);
        }
    }
}
