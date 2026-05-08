using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // ✅ THÊM NÀY
using Newtonsoft.Json;
using System.Text;

namespace BanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ChatController> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message?.Trim()))
                return Ok(new { reply = "Xin chào! Bạn cần tư vấn gì ạ? 😊" });

            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("Gemini API key not configured");
                    return FallbackResponse(request.Message);
                }

                var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var systemPrompt = @"Bạn là nhân viên tư vấn bán hàng chuyên nghiệp của Nội Thất Hưng Hạnh:
- Chuyên nội thất nhựa Đài Loan chính hãng
- Trả lời NGẮN GỌN (2-3 câu), THÂN THIỆN, LỊCH SỰ
- Luôn bằng TIẾNG VIỆT
- Kết thúc bằng 'Hotline: 0968850604' nếu cần tư vấn chi tiết
- Sản phẩm: bền 10 năm, giá 500k-5tr, freeship Gia Lai

Câu hỏi: ";

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = systemPrompt + request.Message } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.8,
                        maxOutputTokens = 150,
                    }
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Gemini [{response.StatusCode}]: {result}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Gemini Error: {result}");
                    return FallbackResponse(request.Message);
                }

                dynamic data = JsonConvert.DeserializeObject(result);

                if (data?.candidates?[0]?.content?.parts?[0]?.text != null)
                {
                    var reply = data.candidates[0].content.parts[0].text.ToString()
                        .Replace("**", "").Replace("*", "").Replace("`", "").Trim();

                    if (!reply.Contains("0968850604"))
                        reply += "\n\nHotline: 0968850604";

                    return Ok(new { reply });
                }

                return FallbackResponse(request.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat system error");
                return FallbackResponse(request.Message);
            }
        }

        private IActionResult FallbackResponse(string message)
        {
            var msgLower = message.ToLower().Trim();

            // 🔥 KEYWORDS THÔNG MINH
            if (msgLower.Contains("ghế") || msgLower.Contains("gaming") || msgLower.Contains("gamming") || msgLower.Contains("ngồi"))
                return Ok(new { reply = @"🪑 **GHẾ NHỰA ĐÀI LOAN:**
- Ghế gaming, ghế ăn, ghế sofa, ghế đẩu
- Giá 800k-3.5tr 
- Chống trơn, chịu lực 150kg
Hotline: 0968850604" });

            if (msgLower.Contains("bàn") || msgLower.Contains("bep") || msgLower.Contains("làm việc"))
                return Ok(new { reply = @"📖 **BÀN NHỰA:**
- Bàn ăn 4-6 người, bàn học, bàn máy tính
- Giá 1.2-4.5tr
- Mặt bàn dày 4cm
Hotline: 0968850604" });

            if (msgLower.Contains("giường") || msgLower.Contains("nằm") || msgLower.Contains("ngủ"))
                return Ok(new { reply = @"🛏️ **GIƯỜNG NHỰA:**
- Giường đơn, giường tầng trẻ em
- Giá 2.5-5.5tr
- Lò xo chống xệ
Hotline: 0968850604" });

            if (msgLower.Contains("tủ") || msgLower.Contains("quần áo") || msgLower.Contains("đồ"))
                return Ok(new { reply = @"👗 **TỦ NHỰA:**
- Tủ quần áo 2-5 cánh, tủ giày
- Giá 1.8-4tr
- Khóa an toàn, không mùi
Hotline: 0968850604" });

            if (msgLower.Contains("giá") || msgLower.Contains("bao nhiêu") || msgLower.Contains("thanh toán"))
                return Ok(new { reply = @"💰 **BẢNG GIÁ CHI TIẾT:**
- Ghế: 800k-3.5tr
- Bàn: 1.2-4.5tr  
- Tủ: 1.8-4tr
- Giường: 2.5-5.5tr
✅ Freeship Gia Lai!
Hotline: 0968850604" });

            if (msgLower.Contains("giao") || msgLower.Contains("ship") || msgLower.Contains("giao hàng"))
                return Ok(new { reply = @"🚚 **GIAO HÀNG:**
- 🎁 Freeship Gia Lai (đơn >2tr)
- 🇻🇳 Toàn quốc +200k-500k
- ⚡ Giao 2-3 ngày
Hotline: 0968850604" });

            if (msgLower.Contains("khuyến mãi") || msgLower.Contains("sale") || msgLower.Contains("giảm"))
                return Ok(new { reply = @"🎉 **ƯU ĐÃI HOT:**
- Mua combo giảm 10-15%
- Freeship Gia Lai
- Tặng quà vận chuyển
⏰ Số lượng có hạn!
Hotline: 0968850604" });

            // Default
            var defaults = new[]
            {
                "🏠 **NỘI THẤT HƯNG HẠNH** chuyên nhựa Đài Loan chính hãng!\nGiá sỉ tốt, bền 10+ năm\nHotline: 0968850604",
                "📞 **TƯ VẤN 24/7:** Ghé cửa hàng Gia Lai hoặc gọi ngay 0968850604!",
                "⭐ **TOP SẢN PHẨM:** Ghế gaming, bàn ăn, tủ quần áo\nFreeship Gia Lai!\nHotline: 0968850604"
            };

            return Ok(new { reply = defaults[new Random().Next(defaults.Length)] });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}