using System.Security.Claims;
using BanHang.Models;
using BanHang.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly IDonHangService _donHangService;

        public DonHangController(IDonHangService donHangService)
        {
            _donHangService = donHangService;
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var donHangs = await _donHangService.GetMyOrdersAsync(userId);
            return View(donHangs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var donHang = await _donHangService.GetOrderDetailsAsync(id, userId);

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, error) = await _donHangService.CancelOrderAsync(id, userId);

            if (!success)
                TempData["error"] = error;
            else
                TempData["success"] = "Hủy đơn thành công!";

            return RedirectToAction("MyOrders", "DonHang");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhGia(int MaSanPham, string NoiDung, int Diem)
        {
            var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "Khách hàng";
            var (success, error) = await _donHangService.SubmitDanhGiaAsync(MaSanPham, Diem, NoiDung, userName);

            if (!success)
                TempData["error"] = error;
            else
                TempData["success"] = $"Cảm ơn {userName}! Đánh giá của bạn đã được gửi.";

            return RedirectToAction("Review", "Product", new { id = MaSanPham });
        }
    }
}