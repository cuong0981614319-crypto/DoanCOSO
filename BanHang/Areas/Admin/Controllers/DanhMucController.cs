using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DanhMucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhMucController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhMucs = await _context.DanhMucs
                .OrderBy(x => x.MaDanhMuc)
                .ToListAsync();

            return View(danhMucs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.DanhMucs.Add(model);
            await _context.SaveChangesAsync();

            TempData["success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DanhMuc model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var danhMuc = await _context.DanhMucs.FindAsync(model.MaDanhMuc);
            if (danhMuc == null) return NotFound();

            danhMuc.TenDanhMuc = model.TenDanhMuc;

            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var danhMuc = await _context.DanhMucs
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.MaDanhMuc == id);

            if (danhMuc == null)
            {
                return NotFound();
            }

            if (danhMuc.SanPhams != null && danhMuc.SanPhams.Any())
            {
                TempData["error"] = "Không thể xoá danh mục này vì đang có sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}