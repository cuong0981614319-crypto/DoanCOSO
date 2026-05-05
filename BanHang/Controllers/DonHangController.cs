using System.Security.Claims;
using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHangs = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)           // Nạp chi tiết đơn hàng
                    .ThenInclude(ct => ct.SanPham)        // Nạp sản phẩm để lấy HinhAnh và TenSanPham
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            donHangs.ForEach(x => x.NgayDat = x.NgayDat.AddHours(7));
            return View(donHangs);
        }

        public async Task<IActionResult> Details(int id)
        {
            // 1. Lấy ID người dùng hiện tại đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Truy vấn đơn hàng và nạp các bảng liên quan (Eager Loading)
            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)        // Nạp chi tiết đơn hàng
                    .ThenInclude(ct => ct.SanPham)     // Nạp thông tin sản phẩm trong chi tiết
                .Include(x => x.LichSuDonHangs)        // <--- QUAN TRỌNG: Nạp lịch sử trạng thái ở đây
                .FirstOrDefaultAsync(x => x.MaDonHang == id && x.UserId == userId);

            // 3. Kiểm tra nếu đơn hàng không tồn tại hoặc không thuộc về user này
            if (donHang == null)
            {
                return NotFound();
            }

            // 4. Trả về View cùng với dữ liệu đã nạp đủ
            return View(donHang);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == id && x.UserId == userId);

            if (donHang == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            // ❌ Không cho hủy nếu đã xử lý
            if (donHang.TrangThai != "Chờ xác nhận" && donHang.TrangThai != "Chờ thanh toán")
            {
                TempData["error"] = "Đơn hàng không thể hủy.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            // ❌ Không cho hủy nếu đã thanh toán
            if (donHang.DaThanhToan)
            {
                TempData["error"] = "Đơn đã thanh toán, không thể hủy.";
                return RedirectToAction("MyOrders", "DonHang");
            }

            // ✅ ĐỔI TRẠNG THÁI
            donHang.TrangThai = "Đã hủy";

            await _context.SaveChangesAsync();

            TempData["success"] = "Hủy đơn thành công!";
            return RedirectToAction("MyOrders", "DonHang");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhGia(int MaSanPham, string NoiDung, int Diem)
        {
            try
            {
                // ✅ Validation đầu vào
                if (Diem < 1 || Diem > 5)
                {
                    TempData["error"] = "Điểm đánh giá phải từ 1-5 sao!";
                    return RedirectToAction("Review", "Product", new { id = MaSanPham });
                }

                if (string.IsNullOrWhiteSpace(NoiDung) || NoiDung.Length < 10)
                {
                    TempData["error"] = "Nhận xét phải có ít nhất 10 ký tự!";
                    return RedirectToAction("Review", "Product", new { id = MaSanPham });
                }

                if (NoiDung.Length > 500)
                {
                    TempData["error"] = "Nhận xét không được quá 500 ký tự!";
                    return RedirectToAction("Review", "Product", new { id = MaSanPham });
                }

                // ✅ Lấy thông tin user hiện tại
                var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "Khách hàng";

                // ✅ Tạo đánh giá mới
                var danhGia = new DanhGia
                {
                    SanPhamId = MaSanPham,
                    TenNguoiDung = userName,
                    Diem = Diem,
                    NoiDung = NoiDung.Trim(),
                    NgayTao = DateTime.Now
                };

                // ✅ Kiểm tra đã đánh giá chưa (tùy chọn)
                var daDanhGia = await _context.DanhGias
                    .AnyAsync(d => d.SanPhamId == MaSanPham && d.TenNguoiDung == userName);

                if (daDanhGia)
                {
                    TempData["error"] = "Bạn đã đánh giá sản phẩm này rồi!";
                    return RedirectToAction("Review", "Product", new { id = MaSanPham });
                }

                // ✅ Lưu vào DB
                _context.DanhGias.Add(danhGia);
                await _context.SaveChangesAsync();

                // ✅ Thành công - Quay về trang Review (không phải Details)
                TempData["success"] = $"Cảm ơn {userName}! Đánh giá của bạn đã được gửi.";
                return RedirectToAction("Review", "Product", new { id = MaSanPham });
            }
            catch (Exception ex)
            {
                // ✅ Log lỗi (tùy chọn)
                TempData["error"] = "Có lỗi xảy ra khi gửi đánh giá. Vui lòng thử lại!";
                return RedirectToAction("Review", "Product", new { id = MaSanPham });
            }
        }
    }
}