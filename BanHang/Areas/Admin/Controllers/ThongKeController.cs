using BanHang.Models;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly IThongKeRepository _repo;

        public ThongKeController(IThongKeRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var model = await _repo.GetThongKeAsync();
            model.DoanhThuTheoNgay = await _repo.GetDoanhThuTheoNgayAsync(fromDate, toDate);

            var thongKeDanhMuc = await _repo.GetThongKeDanhMucAsync();
            ViewBag.Labels = thongKeDanhMuc.Select(x => x.Label).ToList();
            ViewBag.Counts = thongKeDanhMuc.Select(x => x.Count).ToList();

            return View(model);
        }
    }
}