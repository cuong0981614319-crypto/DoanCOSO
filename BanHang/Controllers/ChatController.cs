using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace BanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;

        public ChatController(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        private string GetLocalFallbackResponse(string message, string danhMucData)
        {
            if (string.IsNullOrWhiteSpace(message)) return "Dạ, bạn cần hỗ trợ gì ạ?";
            
            var msg = message.ToLower();
            if (msg.Contains("chào") || msg.Contains("hi ") || msg == "hi" || msg.Contains("hello"))
                return "Xin chào! Mình là trợ lý tự động của Nội Thất Hưng Hạnh. Hiện tại hệ thống AI đang quá tải lượt dùng miễn phí, nhưng mình có thể giúp bạn xem danh mục hoặc địa chỉ cửa hàng. Bạn cần hỗ trợ gì ạ?";
            
            if (msg.Contains("địa chỉ") || msg.Contains("ở đâu"))
                return "Cửa hàng Nội Thất Hưng Hạnh có địa chỉ tại Gia Lai. Rất hân hạnh được đón tiếp bạn!";
                
            if (msg.Contains("điện thoại") || msg.Contains("liên hệ") || msg.Contains("hotline") || msg.Contains("sđt") || msg.Contains("số điện thoại"))
                return "Bạn có thể liên hệ ngay hotline: **0968850604** hoặc email: myhanh71298@gmail.com nhé.";
                
            if (msg.Contains("sản phẩm") || msg.Contains("danh mục") || msg.Contains("bán gì") || msg.Contains("có gì"))
                return "Cửa hàng chuyên cung cấp nội thất nhựa Đài Loan. Các danh mục gồm: " + danhMucData + ". Bạn tham khảo thêm trên menu website nhé.";
                
            return "Dạ, hiện tại AI của cửa hàng đang hết lượt phản hồi tự động. Bạn vui lòng liên hệ trực tiếp hotline **0968850604** để nhân viên tư vấn chi tiết cho bạn nhé!";
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { reply = "Tin nhắn không được để trống." });
            }

            var danhMucs = await _context.DanhMucs.Select(d => d.TenDanhMuc).Distinct().ToListAsync();
            string danhMucData = danhMucs.Count > 0 ? string.Join(", ", danhMucs) : "Chưa có danh mục";

            if (string.IsNullOrWhiteSpace(_geminiApiKey) || _geminiApiKey == "YOUR_GEMINI_API_KEY_HERE" || _geminiApiKey.Length < 10)
            {
                return Ok(new { reply = GetLocalFallbackResponse(request.Message, danhMucData) });
            }

            try
            {
                var allSanPhams = await _context.SanPhams
                    .Select(s => new {
                        s.TenSanPham,
                        GiaGoc = s.Gia,
                        GiaGiam = s.GiaKhuyenMai,
                        DanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : string.Empty,
                        s.MoTa,
                        s.MauSac,
                        s.kichthuc,
                        s.chatlieu
                    })
                    .ToListAsync();

                string productData = JsonSerializer.Serialize(
                    allSanPhams,
                    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                string thongTinCuaHang = @"
Tên cửa hàng: Nội Thất Hưng Hạnh
Chuyên: cung cấp các sản phẩm nội thất nhựa Đài Loan chất lượng cao
Địa chỉ: Gia Lai
Hotline: 0968850604
Email: myhanh71298@gmail.com
Chính sách: Bảo hành, đổi trả, giao hàng toàn quốc
";

                string systemPrompt = $@"
Bạn là trợ lý tư vấn bán hàng AI của cửa hàng Nội Thất Hưng Hạnh.

Thông tin cửa hàng:
{thongTinCuaHang}

Danh mục sản phẩm:
{danhMucData}

Sản phẩm nổi bật:
{productData}

Yêu cầu:
- Trả lời bằng tiếng Việt.
- Thân thiện, ngắn gọn, dễ hiểu.
- Nếu khách hỏi sản phẩm thì tư vấn theo dữ liệu có sẵn.
- Nếu không có thông tin thì bảo khách liên hệ hotline 0968850604.
- Không trả lời lan man ngoài lĩnh vực nội thất.
- Có thể dùng **chữ đậm** cho ý quan trọng.

Tin nhắn khách hàng:
{request.Message}
";

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

                var payload = new
                {
                    contents = new[]
                    {
                        new { role = "user", parts = new[] { new { text = systemPrompt } } }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to local response if AI quota exceeded or unavailable
                    return Ok(new { reply = GetLocalFallbackResponse(request.Message, danhMucData) });
                }

                using var document = JsonDocument.Parse(responseString);

                if (document.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];

                    if (candidate.TryGetProperty("content", out var responseContent) &&
                        responseContent.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var text))
                    {
                        var reply = text.GetString();
                        return Ok(new { reply = reply });
                    }
                }

                return Ok(new { reply = GetLocalFallbackResponse(request.Message, danhMucData) });
            }
            catch (Exception)
            {
                return Ok(new { reply = GetLocalFallbackResponse(request.Message, danhMucData) });
            }
        }
    }
}