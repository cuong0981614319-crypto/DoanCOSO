using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BanHang.Models;

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

        // Vào /Admin/Product sẽ tự chuyển sang /Admin/Product/Create
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.DanhMucs.ToListAsync();
            ViewBag.MaDanhMuc = new SelectList(categories, "MaDanhMuc", "TenDanhMuc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham p)
        {
            if (ModelState.IsValid)
            {
                if (p.ImageFile != null && p.ImageFile.Length > 0)
                {
                    p.HinhAnh = await SaveImage(p.ImageFile);
                }

                _context.SanPhams.Add(p);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công!";

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            var categories = await _context.DanhMucs.ToListAsync();
            ViewBag.MaDanhMuc = new SelectList(categories, "MaDanhMuc", "TenDanhMuc", p.MaDanhMuc);

            return View(p);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.SanPhams.FindAsync(id);
            if (p == null) return NotFound();

            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", p.MaDanhMuc);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPham p)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", p.MaDanhMuc);
                return View(p);
            }

            var existingProduct = await _context.SanPhams.FindAsync(p.MaSanPham);
            if (existingProduct == null) return NotFound();

            try
            {
                existingProduct.TenSanPham = p.TenSanPham;
                existingProduct.Gia = p.Gia;
                existingProduct.MoTa = p.MoTa;
                existingProduct.SoLuong = p.SoLuong;
                existingProduct.MaDanhMuc = p.MaDanhMuc;

                if (p.ImageFile != null && p.ImageFile.Length > 0)
                {
                    existingProduct.HinhAnh = await SaveImage(p.ImageFile);
                }

                await _context.SaveChangesAsync();

                TempData["success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", p.MaDanhMuc);
                return View(p);
            }
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

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploads = Path.Combine(_env.WebRootPath, "images/products");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/images/products/" + fileName;
        }

        private bool ProductExists(int id)
        {
            return _context.SanPhams.Any(e => e.MaSanPham == id);
        }
    }
}