using BanHang.Models;
using BanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DanhMucController : Controller
    {
        private readonly IDanhMucRepository _repo;

        public DanhMucController(IDanhMucRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc model)
        {
            if (!ModelState.IsValid) return View(model);

            await _repo.AddAsync(model);
            TempData["success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var danhMuc = await _repo.GetByIdAsync(id);
            if (danhMuc == null) return NotFound();
            return View(danhMuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DanhMuc model)
        {
            if (!ModelState.IsValid) return View(model);

            await _repo.UpdateAsync(model);
            TempData["success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);

            if (!result)
                TempData["error"] = "Không thể xoá danh mục này vì đang có sản phẩm.";
            else
                TempData["success"] = "Xóa danh mục thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}