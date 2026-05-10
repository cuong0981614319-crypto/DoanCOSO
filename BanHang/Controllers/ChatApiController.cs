using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using BanHang.Models;

namespace BanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public ChatController(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return Ok(new
                    {
                        reply = "Xin chào 👋\nBạn cần hỗ trợ gì về sản phẩm nội thất?"
                    });
                }

                // =========================
                // API KEY
                // =========================
                var apiKey = _config["OpenRouter:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Ok(new
                    {
                        reply = "❌ Thiếu API Key OpenRouter"
                    });
                }

                // =========================
                // LẤY SẢN PHẨM TỪ DATABASE
                // =========================
                var keyword = request.Message.ToLower();

                var products = await _context.SanPhams
                    .Include(x => x.DanhGias)
                    .Where(x =>
                        x.TenSanPham.ToLower().Contains(keyword) ||
                        (x.MoTa != null && x.MoTa.ToLower().Contains(keyword)) ||
                        (x.chatlieu != null && x.chatlieu.ToLower().Contains(keyword))
                    )
                    .OrderByDescending(x => x.DaBan)
                    .Take(10)
                    .ToListAsync();

                // Nếu user không nhập đúng keyword
                if (products.Count == 0)
                {
                    products = await _context.SanPhams
                        .Include(x => x.DanhGias)
                        .OrderByDescending(x => x.DaBan)
                        .Take(10)
                        .ToListAsync();
                }

                // =========================
                // FORMAT DỮ LIỆU SẢN PHẨM
                // =========================
                var productText = string.Join("\n\n", products.Select(p =>
                {
                    double avgRating = 0;

                    if (p.DanhGias != null && p.DanhGias.Count > 0)
                    {
                        avgRating = p.DanhGias.Average(x => x.Diem);
                    }

                    return
$@"Tên sản phẩm: {p.TenSanPham}
Giá: {p.Gia:N0} VNĐ
Chất liệu: {p.chatlieu}
Kích thước: {p.kichthuc}
Mô tả: {p.MoTa}
Đã bán: {p.DaBan}
Đánh giá: {avgRating:0.0}/5";
                }));

                // =========================
                // PROMPT AI
                // =========================
                var messages = new List<object>
                {
                    new
                    {
                        role = "system",
                        content =
$@"
Bạn là AI tư vấn nội thất của cửa hàng Hưng Hạnh.

QUY TẮC:
- Chỉ tư vấn những sản phẩm có trong DATABASE được cung cấp
- Không tự bịa sản phẩm
- Không tạo giá fake
- Không nói sản phẩm ngoài dữ liệu
- Ưu tiên sản phẩm bán chạy và đánh giá cao
- Trả lời ngắn gọn, tự nhiên, dễ hiểu bằng tiếng Việt

DANH SÁCH SẢN PHẨM:

{productText}
"
                    },

                    new
                    {
                        role = "user",
                        content = request.Message
                    }
                };

                // =========================
                // CALL OPENROUTER
                // =========================
                var body = new
                {
                    model = "meta-llama/llama-3.1-8b-instruct",
                    messages = messages,
                    max_tokens = 400
                };

                var json = JsonConvert.SerializeObject(body);

                var client = _httpClientFactory.CreateClient();

                var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions"
                );

                httpRequest.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                httpRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        apiKey.Trim()
                    );

                httpRequest.Headers.TryAddWithoutValidation(
                    "HTTP-Referer",
                    "https://localhost:7156"
                );

                httpRequest.Headers.TryAddWithoutValidation(
                    "X-Title",
                    "BanHangAI"
                );

                var response = await client.SendAsync(httpRequest);

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("OPENROUTER RESPONSE:");
                Console.WriteLine(result);

                if (!response.IsSuccessStatusCode)
                {
                    return Ok(new
                    {
                        reply = "❌ Lỗi AI: " + result
                    });
                }

                var jsonObj = JObject.Parse(result);

                var reply =
                    jsonObj["choices"]?[0]?["message"]?["content"]?.ToString()
                    ?? "AI chưa phản hồi";

                return Ok(new
                {
                    reply = reply
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    reply = "❌ Lỗi: " + ex.Message
                });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}