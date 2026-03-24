using Microsoft.AspNetCore.Mvc;
using BanHang.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhuVucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhuVucController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.KhuVucHienThis.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(KhuVucHienThi kv)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kv);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kv);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var kv = await _context.KhuVucHienThis.FindAsync(id);
            return View(kv);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(KhuVucHienThi kv)
        {
            _context.Update(kv);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var khuVuc = await _context.KhuVucHienThis
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.Id == id);

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

            _context.KhuVucHienThis.Remove(khuVuc);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa khu vực thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}