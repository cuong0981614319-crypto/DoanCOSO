using BanHang.Extensions;
using BanHang.Models;
using BanHang.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace BanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MoMoService _moMoService;
        private readonly VNPayService _vnPayService;
        private const string CartKey = "CART";

        public CartController(
            ApplicationDbContext context,
            MoMoService moMoService,
            VNPayService vnPayService)
        {
            _context = context;
            _moMoService = moMoService;
            _vnPayService = vnPayService;
        }

        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartKey);
            return cart ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(CartKey, cart);
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(GetCart());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1, string? returnUrl = null)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            var product = await _context.SanPhams
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item != null)
            {
                item.SoLuong += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MaSanPham = product.MaSanPham,
                    TenSanPham = product.TenSanPham,
                    Gia = product.Gia,
                    SoLuong = quantity,
                    HinhAnh = product.HinhAnh
                });
            }

            SaveCart(cart);
            TempData["success"] = "Đã thêm vào giỏ hàng!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCartAjax(int id, int quantity = 1)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    redirectToLogin = true,
                    loginUrl = Url.Page("/Account/Login", new
                    {
                        area = "Identity",
                        returnUrl = Url.Action("Index", "Home")
                    })
                });
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            var product = await _context.SanPhams
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sản phẩm không tồn tại."
                });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item != null)
            {
                item.SoLuong += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MaSanPham = product.MaSanPham,
                    TenSanPham = product.TenSanPham,
                    Gia = product.Gia,
                    SoLuong = quantity,
                    HinhAnh = product.HinhAnh
                });
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng!",
                cartCount = cart.Sum(x => x.SoLuong),
                cartTotal = cart.Sum(x => x.ThanhTien),
                items = cart.Select(x => new
                {
                    tenSanPham = x.TenSanPham,
                    soLuong = x.SoLuong,
                    thanhTien = x.ThanhTien,
                    hinhAnh = x.HinhAnh
                }).ToList()
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var product = await _context.SanPhams
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (product == null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["error"] = "Sản phẩm không còn tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.SoLuong = quantity;
            }

            SaveCart(cart);
            TempData["success"] = "Đã cập nhật số lượng.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartKey);
            TempData["success"] = "Đã xoá toàn bộ giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["error"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.GioHang = cart;
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);

            return View(new ThanhToan());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(ThanhToan model)
        {
            var cart = GetCart();

            if (cart == null || !cart.Any())
            {
                TempData["error"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.GioHang = cart;
                ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
                return View(model);
            }

            foreach (var item in cart)
            {
                var sanPham = await _context.SanPhams.FindAsync(item.MaSanPham);
                if (sanPham == null)
                {
                    TempData["error"] = $"Sản phẩm \"{item.TenSanPham}\" không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHang = new DonHang
            {
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                NgayDat = DateTime.UtcNow.AddHours(7),
                TongTien = cart.Sum(x => x.ThanhTien),
                TrangThai = (model.PhuongThucThanhToan == "MOMO"
                             || model.PhuongThucThanhToan == "Bank"
                             || model.PhuongThucThanhToan == "VNPAY")
                    ? "Chờ Thanh Toán"
                    : "Chờ xác nhận",
                UserId = userId,
                PhuongThucThanhToan = model.PhuongThucThanhToan,
                DaThanhToan = false
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            donHang.MaChuyenKhoan = $"DH{donHang.MaDonHang}";
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSanPham = item.MaSanPham,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                });
            }

            await _context.SaveChangesAsync();

            if (model.PhuongThucThanhToan == "MOMO")
            {
                var payUrl = await _moMoService.CreatePaymentAsync(
                    donHang.MaDonHang.ToString(),
                    (long)donHang.TongTien,
                    $"Thanh toán đơn hàng #{donHang.MaDonHang}"
                );

                if (string.IsNullOrWhiteSpace(payUrl))
                {
                    donHang.TrangThai = "Lỗi tạo thanh toán";
                    await _context.SaveChangesAsync();

                    TempData["error"] = "Không tạo được link MoMo";
                    return RedirectToAction(nameof(Checkout));
                }

                return Redirect(payUrl);
            }

            if (model.PhuongThucThanhToan == "VNPAY")
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    HttpContext,
                    donHang.MaDonHang,
                    donHang.TongTien,
                    $"Thanh toan don hang {donHang.MaDonHang}"
                );

                return Redirect(paymentUrl);
            }

            if (model.PhuongThucThanhToan == "Bank")
            {
                return RedirectToAction(nameof(BankTransfer), new { id = donHang.MaDonHang });
            }

            if (model.PhuongThucThanhToan == "COD")
            {
                foreach (var item in cart)
                {
                    var sp = await _context.SanPhams.FindAsync(item.MaSanPham);
                    if (sp != null)
                    {
                        sp.DaBan += item.SoLuong;
                    }
                }

                donHang.TrangThai = "Chờ xác nhận";

                await _context.SaveChangesAsync();
                await SendOrderEmail(donHang, cart);

                HttpContext.Session.Remove(CartKey);

                return RedirectToAction(nameof(Success), new { id = donHang.MaDonHang });
            }

            TempData["error"] = "Phương thức thanh toán không hợp lệ.";
            return RedirectToAction(nameof(Checkout));
        }

        [Authorize]
        public IActionResult Success(int id)
        {
            ViewBag.MaDonHang = id;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Detail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .ThenInclude(x => x.SanPham)
                .FirstOrDefaultAsync(x => x.MaDonHang == id && x.UserId == userId);

            if (donHang == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            return View("~/Views/DonHang/Details.cshtml", donHang);
        }

        private Task SendOrderEmail(DonHang donHang, List<CartItem> cart)
        {
            string body = $"Đơn hàng #{donHang.MaDonHang}\n";
            body += $"Khách: {donHang.HoTen}\n";
            body += $"Số điện thoại: {donHang.SoDienThoai}\n";
            body += $"Địa chỉ: {donHang.DiaChi}\n";
            body += $"Tổng tiền: {donHang.TongTien:N0} VNĐ\n";
            body += $"Trạng thái: {donHang.TrangThai}\n\n";

            foreach (var item in cart)
            {
                body += $"- {item.TenSanPham} x {item.SoLuong} = {item.ThanhTien:N0} VNĐ\n";
            }

            Console.WriteLine(body);
            return Task.CompletedTask;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MomoReturn()
        {
            if (!_moMoService.VerifyReturnUrlSignature(Request.Query))
            {
                TempData["error"] = "Chữ ký thanh toán không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var resultCode = Request.Query["resultCode"].ToString();
            var orderId = Request.Query["orderId"].ToString();

            if (!int.TryParse(orderId, out int maDonHang))
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == maDonHang);

            if (donHang == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (resultCode == "0")
            {
                if (donHang.TrangThai != "Đã thanh toán")
                {
                    donHang.TrangThai = "Đã thanh toán";
                    donHang.DaThanhToan = true;
                    donHang.NgayThanhToan = DateTime.UtcNow.AddHours(7);

                    foreach (var chiTiet in donHang.ChiTietDonHangs)
                    {
                        var sanPham = await _context.SanPhams.FindAsync(chiTiet.MaSanPham);
                        if (sanPham != null)
                        {
                            sanPham.DaBan += chiTiet.SoLuong;
                        }
                    }

                    await _context.SaveChangesAsync();

                    var cart = GetCart();
                    await SendOrderEmail(donHang, cart);
                    HttpContext.Session.Remove(CartKey);
                }

                return RedirectToAction(nameof(Success), new { id = donHang.MaDonHang });
            }

            donHang.TrangThai = "Thanh toán thất bại";
            await _context.SaveChangesAsync();

            TempData["error"] = "Thanh toán MoMo thất bại hoặc bị huỷ.";
            return RedirectToAction(nameof(Checkout));
        }

        [HttpPost]
        public async Task<IActionResult> MomoIpn()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest(new { message = "Empty body" });
            }

            var momoIpn = JsonSerializer.Deserialize<MoMoIpnRequest>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (momoIpn == null)
            {
                return BadRequest(new { message = "Invalid payload" });
            }

            if (!_moMoService.VerifyIpnSignature(momoIpn))
            {
                return BadRequest(new { message = "Invalid signature" });
            }

            if (!int.TryParse(momoIpn.OrderId, out int maDonHang))
            {
                return BadRequest(new { message = "Invalid orderId" });
            }

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == maDonHang);

            if (donHang == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (momoIpn.ResultCode == 0)
            {
                if (donHang.TrangThai != "Đã thanh toán")
                {
                    donHang.TrangThai = "Đã thanh toán";
                    donHang.DaThanhToan = true;
                    donHang.NgayThanhToan = DateTime.UtcNow.AddHours(7);

                    foreach (var chiTiet in donHang.ChiTietDonHangs)
                    {
                        var sanPham = await _context.SanPhams.FindAsync(chiTiet.MaSanPham);
                        if (sanPham != null)
                        {
                            sanPham.DaBan += chiTiet.SoLuong;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                if (donHang.TrangThai != "Đã thanh toán")
                {
                    donHang.TrangThai = "Thanh toán thất bại";
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new
            {
                partnerCode = momoIpn.PartnerCode,
                requestId = momoIpn.RequestId,
                orderId = momoIpn.OrderId,
                resultCode = 0,
                message = "Confirm Success"
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VnpayReturn()
        {
            if (!_vnPayService.ValidateSignature(Request.Query, out var responseData))
            {
                TempData["error"] = "Chữ ký không hợp lệ.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            var orderId = responseData["vnp_TxnRef"];
            var responseCode = responseData.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionStatus = responseData.GetValueOrDefault("vnp_TransactionStatus", "");

            if (!int.TryParse(orderId, out int maDonHang))
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == maDonHang);

            if (donHang == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            if (responseCode == "00" && transactionStatus == "00")
            {
                if (!donHang.DaThanhToan)
                {
                    donHang.DaThanhToan = true;
                    donHang.NgayThanhToan = DateTime.UtcNow.AddHours(7);
                    donHang.TrangThai = "Đã thanh toán";

                    foreach (var ct in donHang.ChiTietDonHangs)
                    {
                        var sp = await _context.SanPhams.FindAsync(ct.MaSanPham);
                        if (sp != null)
                        {
                            sp.DaBan += ct.SoLuong;
                        }
                    }

                    await _context.SaveChangesAsync();

                    var cart = GetCart();
                    await SendOrderEmail(donHang, cart);
                    HttpContext.Session.Remove(CartKey);
                }

                TempData["success"] = "Thanh toán thành công.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            donHang.TrangThai = "Thanh toán thất bại";
            await _context.SaveChangesAsync();

            TempData["error"] = $"Thanh toán thất bại. Mã lỗi: {responseCode}";
            return RedirectToAction("MyOrders", "DonHang");
        }

        [HttpGet]
        public async Task<IActionResult> VnpayIpn()
        {
            if (!_vnPayService.ValidateSignature(Request.Query, out var responseData))
            {
                return Json(new { RspCode = "97", Message = "Invalid signature" });
            }

            var orderId = responseData.GetValueOrDefault("vnp_TxnRef", "");
            var responseCode = responseData.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionStatus = responseData.GetValueOrDefault("vnp_TransactionStatus", "");
            var amountRaw = responseData.GetValueOrDefault("vnp_Amount", "0");

            if (!int.TryParse(orderId, out int maDonHang))
            {
                return Json(new { RspCode = "01", Message = "Order not found" });
            }

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == maDonHang);

            if (donHang == null)
            {
                return Json(new { RspCode = "01", Message = "Order not found" });
            }

            if (!long.TryParse(amountRaw, out long vnpAmount))
            {
                return Json(new { RspCode = "04", Message = "Invalid amount" });
            }

            if ((long)(donHang.TongTien * 100) != vnpAmount)
            {
                return Json(new { RspCode = "04", Message = "Invalid amount" });
            }

            if (donHang.DaThanhToan)
            {
                return Json(new { RspCode = "02", Message = "Order already confirmed" });
            }

            if (responseCode == "00" && transactionStatus == "00")
            {
                donHang.DaThanhToan = true;
                donHang.NgayThanhToan = DateTime.UtcNow.AddHours(7);
                donHang.TrangThai = "Đã thanh toán";

                foreach (var ct in donHang.ChiTietDonHangs)
                {
                    var sp = await _context.SanPhams.FindAsync(ct.MaSanPham);
                    if (sp != null)
                    {
                        sp.DaBan += ct.SoLuong;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { RspCode = "00", Message = "Confirm Success" });
            }

            donHang.TrangThai = "Thanh toán thất bại";
            await _context.SaveChangesAsync();

            return Json(new { RspCode = "00", Message = "Confirm Success" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> BankTransfer(int id)
        {
            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(x => x.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBankTransfer(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);

            if (donHang == null)
            {
                return NotFound();
            }

            if (!donHang.DaThanhToan)
            {
                donHang.DaThanhToan = true;
                donHang.NgayThanhToan = DateTime.UtcNow.AddHours(7);
                donHang.TrangThai = "Đã thanh toán";

                foreach (var ct in await _context.ChiTietDonHangs.Where(x => x.MaDonHang == id).ToListAsync())
                {
                    var sp = await _context.SanPhams.FindAsync(ct.MaSanPham);
                    if (sp != null)
                    {
                        sp.DaBan += ct.SoLuong;
                    }
                }

                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove(CartKey);

            TempData["success"] = "Thanh toán thành công!";

            return RedirectToAction(nameof(Success), new { id = donHang.MaDonHang });
        }
    }
}