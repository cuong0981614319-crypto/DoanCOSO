using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .OrderByDescending(p => p.MaSanPham)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDanhMucAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham p)
        {
            if (!ModelState.IsValid)
            {
                await LoadDanhMucAsync(p.MaDanhMuc);
                return View(p);
            }

            if (p.ImageFile != null && p.ImageFile.Length > 0)
            {
                p.HinhAnh = await SaveImage(p.ImageFile);
            }

            _context.SanPhams.Add(p);
            await _context.SaveChangesAsync();

            TempData["success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.SanPhams.FindAsync(id);
            if (p == null)
            {
                return NotFound();
            }

            await LoadDanhMucAsync(p.MaDanhMuc);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPham p)
        {
            if (!ModelState.IsValid)
            {
                await LoadDanhMucAsync(p.MaDanhMuc);
                return View(p);
            }

            try
            {
                if (p.ImageFile != null && p.ImageFile.Length > 0)
                {
                    p.HinhAnh = await SaveImage(p.ImageFile);
                }

                _context.Update(p);
                await _context.SaveChangesAsync();
                TempData["success"] = "Cập nhật thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(p.MaSanPham))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.SanPhams.FindAsync(id);
            if (p != null)
            {
                _context.SanPhams.Remove(p);
                await _context.SaveChangesAsync();
                TempData["success"] = "Xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDanhMucAsync(int? selectedId = null)
        {
            var categories = await _context.DanhMucs
                .AsNoTracking()
                .OrderBy(x => x.TenDanhMuc)
                .ToListAsync();

            ViewBag.MaDanhMuc = new SelectList(categories, "MaDanhMuc", "TenDanhMuc", selectedId);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploads = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploads, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/products/{fileName}";
        }

        private bool ProductExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSanPham == id);
        }
    }
}
