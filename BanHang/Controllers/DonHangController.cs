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
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

           
          
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
    }
}