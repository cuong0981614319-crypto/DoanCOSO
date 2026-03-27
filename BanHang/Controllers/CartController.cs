using BanHang.Extensions;
using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartKey = "CART";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
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

            if (!cart.Any())
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
                var sp = await _context.SanPhams.FindAsync(item.MaSanPham);

                if (sp == null)
                {
                    TempData["error"] = $"Sản phẩm \"{item.TenSanPham}\" không còn tồn tại.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHang = new DonHang
            {
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
               
                DiaChi = model.DiaChi,
               
                NgayDat = DateTime.Now,
                TongTien = cart.Sum(x => x.ThanhTien),
                TrangThai = "Chờ xác nhận",
                UserId = userId
            };

            _context.DonHangs.Add(donHang);
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

                var sp = await _context.SanPhams.FindAsync(item.MaSanPham);
                if (sp != null)
                {
                    sp.DaBan += item.SoLuong;
                }
            }

            await _context.SaveChangesAsync();

            await SendOrderEmail(donHang, cart);

            HttpContext.Session.Remove(CartKey);

            return RedirectToAction(nameof(Success), new { id = donHang.MaDonHang });
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

            return View(donHang);
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
    }
}