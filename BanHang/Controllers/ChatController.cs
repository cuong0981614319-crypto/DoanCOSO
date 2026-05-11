using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;

        public ChatController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message is empty");
            }

            if (string.IsNullOrEmpty(_geminiApiKey) || _geminiApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return Ok(new { reply = "Chưa cấu hình API Key cho Gemini. Vui lòng thêm key hợp lệ vào file appsettings.Development.json tại mục Gemini:ApiKey." });
            }

            try
            {
                // 1. Đọc dữ liệu từ Database để cấp context cho Gemini
                var danhMucs = await _context.DanhMucs.Select(d => d.TenDanhMuc).ToListAsync();
                var danhMucData = string.Join(", ", danhMucs);

                var topSanPhams = await _context.SanPhams
                    .OrderByDescending(s => s.DaBan)
                    .Take(10)
                    .Select(s => new { s.TenSanPham, s.Gia, s.GiaKhuyenMai, s.MoTa })
                    .ToListAsync();
                
                string productData = JsonSerializer.Serialize(topSanPhams, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                var thongTinCuaHang = @"
Tên cửa hàng: Nội Thất Hưng Hạnh
Chuyên: cung cấp các sản phẩm nội thất nhựa Đài Loan chất lượng cao.
Địa chỉ: Gia Lai
Hotline: 0968850604
Email: myhanh71298@gmail.com
Chính sách: Bảo hành, đổi trả, giao hàng toàn quốc.
";

                // 2. Cấu hình Prompt
                string systemPrompt = $@"Bạn là nhân viên tư vấn bán hàng trí tuệ nhân tạo của cửa hàng {thongTinCuaHang}.
Danh mục sản phẩm hiện có: {danhMucData}.
Một số sản phẩm nổi bật: {productData}.
Nhiệm vụ của bạn là tư vấn cho khách hàng một cách thân thiện, ngắn gọn, dễ hiểu và chuyên nghiệp.
Luôn chào hỏi lịch sự nếu là tin nhắn đầu tiên. 
Gợi ý sản phẩm phù hợp nếu khách hỏi. Nếu khách hỏi thông tin ngoài luồng hoặc không có trong dữ liệu, hãy khuyên họ liên hệ hotline 0968850604. 
Hãy định dạng chữ đậm bằng cách bọc nội dung trong ** **. Trả lời bằng tiếng Việt.
Tin nhắn của khách hàng: ";

                // 3. Gọi API Gemini
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_geminiApiKey}";

                var payload = new
                {
                    contents = new[]
                    {
                        new { role = "user", parts = new[] { new { text = systemPrompt + "\n\n" + request.Message } } }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseString);
                    var candidates = document.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var reply = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        return Ok(new { reply = reply });
                    }
                }
                else
                {
                    var errorStr = await response.Content.ReadAsStringAsync();
                    return Ok(new { reply = "Lỗi kết nối tới Gemini: " + response.StatusCode + ". Vui lòng kiểm tra lại cấu hình." });
                }

                return Ok(new { reply = "Xin lỗi, hiện tại hệ thống tư vấn đang bận. Vui lòng liên hệ hotline 0968850604 để được hỗ trợ trực tiếp." });
            }
            catch (Exception ex)
            {
                return Ok(new { reply = "Đã xảy ra lỗi kết nối với trợ lý ảo. Xin vui lòng liên hệ hotline 0968850604." });
            }
        }
    }
}
