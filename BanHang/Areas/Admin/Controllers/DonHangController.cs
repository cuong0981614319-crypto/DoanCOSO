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

            donHang.TrangThai = trangThai;
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
        
    }
}