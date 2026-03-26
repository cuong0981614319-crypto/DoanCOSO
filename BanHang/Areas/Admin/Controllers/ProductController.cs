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
                .Include(x => x.DanhMuc)          // 🔥 load danh mục
                .Include(x => x.KhuVucHienThi)    // 🔥 load khu vực
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
            // 🔥 validate bắt buộc chọn
            if (p.MaDanhMuc == null)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục");

            if (p.KhuVucHienThiId == null)
                ModelState.AddModelError("KhuVucHienThiId", "Vui lòng chọn khu vực hiển thị");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(p);
                return View(p);
            }

            try
            {
                // upload ảnh
                if (p.ImageFiles != null && p.ImageFiles.Any(f => f.Length > 0))
                {
                    var file = p.ImageFiles.First();
                    p.HinhAnh = await SaveImage(file);
                }

                _context.SanPhams.Add(p);
                await _context.SaveChangesAsync();

                TempData["success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                await LoadDropdowns(p);
                return View(p);
            }
        }

        // ===================== EDIT =====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.SanPhams.FindAsync(id);

            if (p == null)
                return NotFound();

            await LoadDropdowns(p);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham p)
        {
            if (id != p.MaSanPham)
                return NotFound();

            // 🔥 validate
            if (p.MaDanhMuc == null)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục");

            if (p.KhuVucHienThiId == null)
                ModelState.AddModelError("KhuVucHienThiId", "Vui lòng chọn khu vực hiển thị");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(p);
                return View(p);
            }

            var existing = await _context.SanPhams.FindAsync(id);
            if (existing == null)
                return NotFound();

            try
            {
                // update dữ liệu
                existing.TenSanPham = p.TenSanPham;
                existing.Gia = p.Gia;
                existing.MoTa = p.MoTa;
                existing.MauSac = p.MauSac;

                // 🔥 QUAN TRỌNG
                existing.MaDanhMuc = p.MaDanhMuc;
                existing.KhuVucHienThiId = p.KhuVucHienThiId;

                // update ảnh nếu có
                if (p.ImageFiles != null && p.ImageFiles.Any(f => f.Length > 0))
                {
                    var file = p.ImageFiles.First();
                    existing.HinhAnh = await SaveImage(file);
                }

                await _context.SaveChangesAsync();

                TempData["success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
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
            }

            return RedirectToAction(nameof(Index));
        }

        // ===================== LOAD DROPDOWN =====================
        private async Task LoadDropdowns(SanPham? p = null)
        {
            ViewBag.MaDanhMuc = new SelectList(
                await _context.DanhMucs.ToListAsync(),
                "MaDanhMuc",
                "TenDanhMuc",
                p?.MaDanhMuc
            );

            ViewBag.KhuVucHienThiId = new SelectList(
                await _context.KhuVucHienThis.ToListAsync(),
                "Id",
                "Ten",
                p?.KhuVucHienThiId
            );
        }

        // ===================== SAVE IMAGE =====================
        private async Task<string> SaveImage(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "images/products");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/products/" + fileName;
        }
    }
}