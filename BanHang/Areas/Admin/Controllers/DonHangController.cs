using BanHang.Models;
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
        private readonly IAdminDonHangService _service;

        public DonHangController(IAdminDonHangService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string status)
        {
            var donHangs = await _service.GetAllAsync(status);
            ViewBag.CurrentStatus = status;
            return View(donHangs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _service.GetByIdWithDetailsAsync(id);
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
            var result = await _service.UpdateStatusAsync(maDonHang, trangThai);
            if (!result) return NotFound();

            TempData["success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction("Details", new { id = maDonHang });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                TempData["error"] = "Không tìm thấy đơn hàng.";
            else
                TempData["success"] = "Xóa đơn hàng thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}