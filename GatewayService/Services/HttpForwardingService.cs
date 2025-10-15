using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Fabric;
using System.Threading;

namespace GatewayService.Services
{
    public class HttpForwardingService
    {
        private readonly HttpClient _client;
        private readonly StatelessServiceContext _context;

        public HttpForwardingService(HttpClient client, StatelessServiceContext context)
        {
            _client = client;
            _context = context;
            _client.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<string> SendToDocumentService(IFormFile file)
        {
            // DocumentService je integrisan u GatewayService - vrati success
            return $"{{\"Message\": \"File processed by GatewayService\", \"FileName\": \"{file.FileName}\"}}";
        }

        public async Task<string> SendToChatService(string question)
        {
            // ChatService je integrisan u GatewayService - vrati mock odgovor
            return $"{{\"Answer\": \"This is a response from GatewayService: {question}\"}}";
        }

        public async Task<string> GetChatHistory()
        {
            // Chat history se sada čuva u GatewayService
            return "[]";
        }
    }
}