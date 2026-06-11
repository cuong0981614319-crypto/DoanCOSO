using BanHang.Models;
using BanHang.Repositories;
using BanHang.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DonHangController : Controller
    {
        private readonly IAdminDonHangRepository _repo;

        public DonHangController(IAdminDonHangRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index(string status)
        {
            var donHangs = await _repo.GetAllAsync(status);
            ViewBag.CurrentStatus = status;
            return View(donHangs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _repo.GetByIdWithDetailsAsync(id);
            if (donHang == null) return NotFound();

            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Chờ xác nhận", Text = "Chờ xác nhận" },
                new SelectListItem { Value = "Đã xác nhận",  Text = "Đã xác nhận"  },
                new SelectListItem { Value = "Đang giao",    Text = "Đang giao"    },
                new SelectListItem { Value = "Hoàn thành",   Text = "Hoàn thành"   },
                new SelectListItem { Value = "Đã hủy",       Text = "Đã hủy"       }
            };

            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int maDonHang, string trangThai)
        {
            var result = await _repo.UpdateStatusAsync(maDonHang, trangThai);
            if (!result) return NotFound();

            TempData["success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction("Details", new { id = maDonHang });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);

            if (!result)
                TempData["error"] = "Không tìm thấy đơn hàng.";
            else
                TempData["success"] = "Xóa đơn hàng thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}