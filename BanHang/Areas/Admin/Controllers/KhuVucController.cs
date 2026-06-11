using BanHang.Models;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhuVucController : Controller
    {
        private readonly IKhuVucRepository _repo;

        public KhuVucController(IKhuVucRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(KhuVucHienThi kv)
        {
            if (!ModelState.IsValid) return View(kv);

            await _repo.AddAsync(kv);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var kv = await _repo.GetByIdWithSanPhamsAsync(id);
            return View(kv);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(KhuVucHienThi kv)
        {
            await _repo.UpdateAsync(kv);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var khuVuc = await _repo.GetByIdWithSanPhamsAsync(id);

            if (khuVuc == null)
            {
                TempData["error"] = "Khu vực không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (khuVuc.SanPhams != null && khuVuc.SanPhams.Any())
            {
                TempData["error"] = $"Không thể xoá khu vực \"{khuVuc.Ten}\" vì đang có {khuVuc.SanPhams.Count} sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.DeleteAsync(id);
            TempData["success"] = "Xóa khu vực thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}