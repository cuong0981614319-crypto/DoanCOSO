using BanHang.Extensions;
using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Index()
        {
            return View(GetCart());
        }

        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _context.SanPhams.FirstOrDefaultAsync(x => x.MaSanPham == id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item != null)
                item.SoLuong += quantity;
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
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.SoLuong = quantity;
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == id);

            if (item != null)
                cart.Remove(item);

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartKey);
            return RedirectToAction(nameof(Index));
        }

        // ================= CHECKOUT =================

        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["error"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            return View(new ThanhToan());
        }

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
                return View(model);
            }

            // 🔥 TẠO ĐƠN HÀNG
            var donHang = new DonHang
            {
                NgayDat = DateTime.Now,
                TongTien = cart.Sum(x => x.ThanhTien),
                HoTenNguoiNhan = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                DiaChiGiaoHang = model.DiaChi,
                Email = model.Email,
                GhiChu = model.GhiChu,
                PhuongThucThanhToan = model.PhuongThucThanhToan,
                TrangThai = "Chờ xử lý"
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            // 🔥 LƯU CHI TIẾT ĐƠN
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
                    sp.SoLuong -= item.SoLuong;
            }

            await _context.SaveChangesAsync();

            // 🔥 GỬI MAIL (tạm console)
            await SendOrderEmail(donHang, cart);

            HttpContext.Session.Remove(CartKey);

            return RedirectToAction("Success", new { id = donHang.MaDonHang });
        }

        // ================= SUCCESS =================

        public IActionResult Success(int id)
        {
            ViewBag.MaDonHang = id;
            return View();
        }

        // ================= DETAIL =================

        public async Task<IActionResult> Detail(int id)
        {
            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .ThenInclude(x => x.SanPham)
                .FirstOrDefaultAsync(x => x.MaDonHang == id);

            return View(donHang);
        }

        // ================= EMAIL =================

        private async Task SendOrderEmail(DonHang donHang, List<CartItem> cart)
        {
            string body = $"Đơn hàng #{donHang.MaDonHang}\n";
            body += $"Khách: {donHang.HoTenNguoiNhan}\n";
            body += $"Thanh toán: {donHang.PhuongThucThanhToan}\n\n";

            foreach (var item in cart)
            {
                body += $"- {item.TenSanPham} x {item.SoLuong}\n";
            }

            Console.WriteLine(body); // test
        }
    }
}