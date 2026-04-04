using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status)
        {
            var query = _context.DonHangs.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Đã Thanh Toán")
                {
                    query = query.Where(x => x.DaThanhToan == true);
                }
                else if (status == "Chờ thanh toán")
                {
                    query = query.Where(x => x.DaThanhToan == false);
                }
                else
                {
                    query = query.Where(x => x.TrangThai == status);
                }
            }

            var donHangs = await query
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            ViewBag.CurrentStatus = status;

            return View(donHangs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(x => x.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Chờ xác nhận", Text = "Chờ xác nhận" },
                new SelectListItem { Value = "Đã xác nhận", Text = "Đã xác nhận" },
                new SelectListItem { Value = "Đang giao", Text = "Đang giao" },
                new SelectListItem { Value = "Hoàn thành", Text = "Hoàn thành" },
                new SelectListItem { Value = "Đã hủy", Text = "Đã hủy" }
            };

            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int maDonHang, string trangThai)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);

            if (donHang == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Nếu chọn Hoàn thành → tự động thanh toán
            if (trangThai == "Hoàn thành")
            {
                donHang.TrangThai = "Hoàn thành";
                donHang.DaThanhToan = true;
                donHang.NgayThanhToan = DateTime.Now;
            }
            else
            {
                donHang.TrangThai = trangThai;
            }

            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == id);

            if (donHang == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            if (donHang.ChiTietDonHangs.Any())
            {
                _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);
            }

            _context.DonHangs.Remove(donHang);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa đơn hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ThongKe()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var donHoanThanh = _context.DonHangs
                .Where(x => x.DaThanhToan );

            var model = new ThongKeDoanhThuViewModel
            {
                DoanhThuHomNay = await donHoanThanh
                    .Where(x => x.NgayDat.Date == today)
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                DoanhThuThangNay = await donHoanThanh
                    .Where(x => x.NgayDat >= firstDayOfMonth)
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                TongDoanhThu = await donHoanThanh
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                SoDonDaThanhToan = await donHoanThanh.CountAsync(),


                DoanhThuTheoNgay = await donHoanThanh
                    .GroupBy(x => x.NgayDat.Date)
                    .Select(g => new DoanhThuTheoNgayItem
                    {
                        Ngay = g.Key,
                        DoanhThu = g.Sum(x => x.TongTien)
                    })
                    .OrderBy(x => x.Ngay)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}