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
                    return Ok(new { reply = "Bạn cần hỗ trợ gì ạ 😊" });
                }

                var apiKey = _config["OpenRouter:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Ok(new { reply = "Thiếu API Key" });
                }

                var client = _httpClientFactory.CreateClient();
                var url = "https://openrouter.ai/api/v1/chat/completions";

                // =========================
                // 🔥 CHỈ LẤY SẢN PHẨM NHỰA TỪ DB
                // =========================
                var products = await _context.SanPhams
                    .Include(x => x.DanhGias)
                    .Where(x =>
                        x.TenSanPham.Contains("tủ") ||
                        x.TenSanPham.Contains("nhựa") ||
                        (x.MoTa != null && x.MoTa.Contains("nhựa"))
                    )
                    .OrderByDescending(x => x.DaBan)
                    .Take(10)
                    .ToListAsync();

                var productText = products.Count == 0
                    ? "Hiện tại chưa có sản phẩm nội thất nhựa nào trong kho."
                    : string.Join("\n\n", products.Select(p =>
                    {
                        var avgRating = (p.DanhGias != null && p.DanhGias.Count > 0)
                            ? p.DanhGias.Average(x => x.Diem)
                            : 0;

                        return
$@"Tên: {p.TenSanPham}
Giá: {p.Gia:N0} VNĐ
Mô tả: {p.MoTa}
Đã bán: {p.DaBan}
Đánh giá: {avgRating:0.0}/5";
                    }));

                // =========================
                // SYSTEM PROMPT KHÓA CHẶT
                // =========================
                var messages = new List<object>
                {
                    new
                    {
                        role = "system",
                        content =
@"Bạn là AI tư vấn nội thất NHỰA của cửa hàng Hưng Hạnh.

QUY TẮC BẮT BUỘC:
- CHỈ tư vấn nội thất NHỰA (tủ nhựa, kệ nhựa, bàn nhựa)
- CHỈ được dùng sản phẩm trong danh sách được cung cấp
- KHÔNG được tự bịa sản phẩm
- KHÔNG được nói về sofa, giường, gỗ, hoặc sản phẩm ngoài DB
- Nếu không có sản phẩm phù hợp → nói:
'Hiện tại cửa hàng chưa có sản phẩm nội thất nhựa phù hợp'"

                    },

                    new
                    {
                        role = "system",
                        content = "DANH SÁCH SẢN PHẨM TRONG DATABASE:\n" + productText
                    },

                    new
                    {
                        role = "user",
                        content = request.Message
                    }
                };

                // =========================
                // CALL AI
                // =========================
                var body = new
                {
                    model = "meta-llama/llama-3.1-8b-instruct",
                    messages,
                    max_tokens = 600
                };

                var json = JsonConvert.SerializeObject(body);

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                httpRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey.Trim());

                httpRequest.Headers.TryAddWithoutValidation("HTTP-Referer", "https://localhost:7156");
                httpRequest.Headers.TryAddWithoutValidation("X-Title", "BanHangAI");

                var response = await client.SendAsync(httpRequest);
                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);

                if (!response.IsSuccessStatusCode)
                {
                    return Ok(new { reply = "Lỗi AI: " + result });
                }

                var jsonObj = JObject.Parse(result);

                var reply = jsonObj["choices"]?[0]?["message"]?["content"]?.ToString()
                            ?? "Không có phản hồi";

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