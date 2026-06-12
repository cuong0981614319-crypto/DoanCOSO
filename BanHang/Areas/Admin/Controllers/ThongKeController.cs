using BanHang.Services;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly IThongKeService _thongKeService;
        private readonly IThongKeRepository _repo; // chỉ dùng cho GetThongKeDanhMucAsync

        public ThongKeController(IThongKeService thongKeService, IThongKeRepository repo)
        {
            _thongKeService = thongKeService;
            _repo = repo;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var model = await _thongKeService.GetThongKeAsync(fromDate, toDate);

            var thongKeDanhMuc = await _repo.GetThongKeDanhMucAsync();
            ViewBag.Labels = thongKeDanhMuc.Select(x => x.Label).ToList();
            ViewBag.Counts = thongKeDanhMuc.Select(x => x.Count).ToList();

            return View(model);
        }
    }
}