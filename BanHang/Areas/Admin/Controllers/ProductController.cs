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

        // ===================== DANH SÁCH =====================
        public async Task<IActionResult> Index()
        {
            var products = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.KhuVucHienThi)
                .ToListAsync();

            return View(products);
        }

        // ===================== CREATE =====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham p)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(p);
                return View(p);
            }

            try
            {
                var imagePaths = new List<string>();

                if (p.ImageFiles != null && p.ImageFiles.Any())
                {
                    foreach (var file in p.ImageFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var path = await SaveImage(file);
                            imagePaths.Add(path);
                        }
                    }
                }

                // lấy ảnh đầu tiên làm ảnh đại diện
                if (imagePaths.Any())
                {
                    p.HinhAnh = imagePaths.First();
                }

                _context.SanPhams.Add(p);
                await _context.SaveChangesAsync();

                if (imagePaths.Any())
                {
                    var dsAnh = imagePaths.Select(path => new HinhAnhSanPham
                    {
                        MaSanPham = p.MaSanPham,
                        DuongDanAnh = path
                    }).ToList();

                    _context.HinhAnhSanPhams.AddRange(dsAnh);
                    await _context.SaveChangesAsync();
                }

                TempData["success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm: " + ex.Message);
                await LoadDropdowns(p);
                return View(p);
            }
        }

        // ===================== EDIT =====================
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.SanPhams.FindAsync(id);
            if (p == null) return NotFound();

            await LoadDropdowns(p);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPham p)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(p);
                return View(p);
            }

            var existingProduct = await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == p.MaSanPham);

            if (existingProduct == null)
            {
                return NotFound();
            }

            try
            {
                existingProduct.TenSanPham = p.TenSanPham;
                existingProduct.Gia = p.Gia;
                existingProduct.MoTa = p.MoTa;
                existingProduct.MauSac = p.MauSac;
                existingProduct.MaDanhMuc = p.MaDanhMuc;
                existingProduct.KhuVucHienThiId = p.KhuVucHienThiId;

                if (p.ImageFiles != null && p.ImageFiles.Any())
                {
                    foreach (var file in p.ImageFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var path = await SaveImage(file);

                            _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                            {
                                MaSanPham = existingProduct.MaSanPham,
                                DuongDanAnh = path
                            });

                            if (string.IsNullOrEmpty(existingProduct.HinhAnh))
                            {
                                existingProduct.HinhAnh = path;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message);
                await LoadDropdowns(p);
                return View(p);
            }
        }

        // ===================== DELETE =====================
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
        // ===================== LOAD DROPDOWN =====================
        private async Task LoadDropdowns(SanPham? p = null)
        {
            var categories = await _context.DanhMucs.ToListAsync();
            var khuVucs = await _context.KhuVucHienThis.ToListAsync();

            ViewBag.MaDanhMuc = new SelectList(categories, "MaDanhMuc", "TenDanhMuc", p?.MaDanhMuc);
            ViewBag.KhuVucHienThiId = new SelectList(khuVucs, "Id", "Ten", p?.KhuVucHienThiId);
        }

        // ===================== SAVE IMAGE =====================
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