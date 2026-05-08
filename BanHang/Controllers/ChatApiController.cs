using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        private const string SESSION_KEY = "CHAT_HISTORY";

        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return Ok(new { reply = "Bạn cần hỗ trợ gì ạ 😊" });
                }

                var apiKey = _config["OpenRouter:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Ok(new { reply = "❌ Thiếu API Key OpenRouter" });
                }

                var client = _httpClientFactory.CreateClient();

                var url = "https://openrouter.ai/api/v1/chat/completions";

                // =========================
                // MEMORY CHAT (SESSION)
                // =========================
                List<object> messages;

                var sessionData = HttpContext.Session.GetString(SESSION_KEY);

                if (string.IsNullOrEmpty(sessionData))
                {
                    messages = new List<object>
                    {
                        new
                        {
                            role = "system",
                            content = "Bạn là nhân viên tư vấn nội thất Hưng Hạnh, trả lời ngắn gọn tiếng Việt"
                        }
                    };
                }
                else
                {
                    messages = JsonConvert.DeserializeObject<List<object>>(sessionData)
                               ?? new List<object>();
                }

                messages.Add(new
                {
                    role = "user",
                    content = request.Message
                });

                var body = new
                {
                    model = "meta-llama/llama-3.1-8b-instruct",
                    messages = messages,
                    max_tokens = 500   // hoặc 1000
                };

                var json = JsonConvert.SerializeObject(body);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // =========================
                // FIX AUTH (QUAN TRỌNG)
                // =========================
                httpRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey.Trim());

                httpRequest.Headers.TryAddWithoutValidation("HTTP-Referer", "https://localhost:7156");
                httpRequest.Headers.TryAddWithoutValidation("X-Title", "BanHangAI");

                var response = await client.SendAsync(httpRequest);
                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("OPENROUTER RESPONSE: " + result);

                if (!response.IsSuccessStatusCode)
                {
                    return Ok(new { reply = "❌ OpenRouter lỗi: " + result });
                }

                var jsonObj = JObject.Parse(result);

                string reply = jsonObj["choices"]?[0]?["message"]?["content"]?.ToString()
                               ?? "AI chưa phản hồi 😢";

                // =========================
                // SAVE MEMORY
                // =========================
                messages.Add(new
                {
                    role = "assistant",
                    content = reply
                });

                HttpContext.Session.SetString(
                    SESSION_KEY,
                    JsonConvert.SerializeObject(messages)
                );

                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                return Ok(new { reply = "Lỗi: " + ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}