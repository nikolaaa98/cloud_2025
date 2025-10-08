namespace GatewayService.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class HttpForwardingService
    {
        private readonly HttpClient _client;
        public HttpForwardingService(HttpClient client) => _client = client;

        public async Task<string> SendToDocumentService(IFormFile file)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
            var response = await _client.PostAsync("http://DocumentService/api/document/upload", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SendToChatService(string question)
        {
            var response = await _client.PostAsJsonAsync("http://ChatService/api/chat/ask", question);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
