using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BanHang.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly Cloudinary _cloudinary;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env, Cloudinary cloudinary)
        {
            _context = context;
            _env = env;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.KhuVucHienThi)
                .OrderByDescending(x => x.MaSanPham)
                .ToListAsync();

            return View(sanPhams);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new SanPham());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham p)
        {
            ValidateProductInput(p);

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(p);
                return View(p);
            }

            try
            {
                var sanPham = new SanPham
                {
                    TenSanPham = p.TenSanPham,
                    Gia = p.Gia,
                    MoTa = p.MoTa,
                    MauSac = p.MauSac,
                    MaDanhMuc = p.MaDanhMuc,
                    KhuVucHienThiId = p.KhuVucHienThiId,
                    HinhAnhSanPhams = new List<HinhAnhSanPham>()
                };

                if (p.ImageFiles != null && p.ImageFiles.Any(f => f.Length > 0))
                {
                    bool isFirstImage = true;

                    foreach (var file in p.ImageFiles.Where(f => f.Length > 0))
                    {
                        var imagePath = await SaveImage(file);

                        if (isFirstImage)
                        {
                            sanPham.HinhAnh = imagePath;
                            isFirstImage = false;
                        }

                        sanPham.HinhAnhSanPhams.Add(new HinhAnhSanPham
                        {
                            DuongDanAnh = imagePath
                        });
                    }
                }

                _context.SanPhams.Add(sanPham);
                await _context.SaveChangesAsync();

                TempData["success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
                await LoadDropdowns(p);
                return View(p);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

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

            ValidateProductInput(p);

            if (!ModelState.IsValid)
            {
                var invalidModel = await _context.SanPhams
                    .Include(x => x.HinhAnhSanPhams)
                    .FirstOrDefaultAsync(x => x.MaSanPham == id);

                if (invalidModel != null)
                {
                    invalidModel.TenSanPham = p.TenSanPham;
                    invalidModel.Gia = p.Gia;
                    invalidModel.MoTa = p.MoTa;
                    invalidModel.MauSac = p.MauSac;
                    invalidModel.MaDanhMuc = p.MaDanhMuc;
                    invalidModel.KhuVucHienThiId = p.KhuVucHienThiId;
                    invalidModel.SelectedThumbnail = p.SelectedThumbnail;
                }

                await LoadDropdowns(invalidModel ?? p);
                return View(invalidModel ?? p);
            }

            var existing = await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (existing == null)
                return NotFound();

            try
            {
                existing.TenSanPham = p.TenSanPham;
                existing.Gia = p.Gia;
                existing.MoTa = p.MoTa;
                existing.MauSac = p.MauSac;
                existing.MaDanhMuc = p.MaDanhMuc;
                existing.KhuVucHienThiId = p.KhuVucHienThiId;

                if (p.DeletedImageIds != null && p.DeletedImageIds.Any())
                {
                    var imagesToDelete = existing.HinhAnhSanPhams!
                        .Where(x => p.DeletedImageIds.Contains(x.Id))
                        .ToList();

                    foreach (var img in imagesToDelete)
                    {
                        DeleteImageFile(img.DuongDanAnh);
                        _context.HinhAnhSanPhams.Remove(img);
                    }

                    await _context.SaveChangesAsync();
                }

                if (p.ImageFiles != null && p.ImageFiles.Any(f => f.Length > 0))
                {
                    foreach (var file in p.ImageFiles.Where(f => f.Length > 0))
                    {
                        var imagePath = await SaveImage(file);

                        _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                        {
                            MaSanPham = existing.MaSanPham,
                            DuongDanAnh = imagePath
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                existing = await _context.SanPhams
                    .Include(x => x.HinhAnhSanPhams)
                    .FirstOrDefaultAsync(x => x.MaSanPham == id);

                if (existing == null)
                    return NotFound();

                if (!string.IsNullOrWhiteSpace(p.SelectedThumbnail))
                {
                    bool selectedExists = existing.HinhAnhSanPhams!
                        .Any(x => x.DuongDanAnh == p.SelectedThumbnail);

                    existing.HinhAnh = selectedExists
                        ? p.SelectedThumbnail
                        : existing.HinhAnhSanPhams.FirstOrDefault()?.DuongDanAnh;
                }
                else
                {
                    bool thumbnailStillExists = !string.IsNullOrWhiteSpace(existing.HinhAnh)
                        && existing.HinhAnhSanPhams!.Any(x => x.DuongDanAnh == existing.HinhAnh);

                    if (!thumbnailStillExists)
                    {
                        existing.HinhAnh = existing.HinhAnhSanPhams.FirstOrDefault()?.DuongDanAnh;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm: " + ex.Message);

                var model = await _context.SanPhams
                    .Include(x => x.HinhAnhSanPhams)
                    .FirstOrDefaultAsync(x => x.MaSanPham == id);

                await LoadDropdowns(model ?? p);
                return View(model ?? p);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (p == null)
                return RedirectToAction(nameof(Index));

            try
            {
                if (p.HinhAnhSanPhams != null && p.HinhAnhSanPhams.Any())
                {
                    foreach (var img in p.HinhAnhSanPhams)
                    {
                        DeleteImageFile(img.DuongDanAnh);
                    }

                    _context.HinhAnhSanPhams.RemoveRange(p.HinhAnhSanPhams);
                }

                if (!string.IsNullOrWhiteSpace(p.HinhAnh))
                {
                    DeleteImageFile(p.HinhAnh);
                }

                _context.SanPhams.Remove(p);
                await _context.SaveChangesAsync();

                TempData["success"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Xóa thất bại: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateProductInput(SanPham p)
        {
            if (p.MaDanhMuc == null || p.MaDanhMuc <= 0)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục");

            if (p.KhuVucHienThiId == null || p.KhuVucHienThiId <= 0)
                ModelState.AddModelError("KhuVucHienThiId", "Vui lòng chọn khu vực hiển thị");

            if (string.IsNullOrWhiteSpace(p.TenSanPham))
                ModelState.AddModelError("TenSanPham", "Vui lòng nhập tên sản phẩm");

            if (p.Gia <= 0)
                ModelState.AddModelError("Gia", "Giá sản phẩm phải lớn hơn 0");
        }

        private async Task LoadDropdowns(SanPham? p = null)
        {
            ViewBag.MaDanhMuc = new SelectList(
                await _context.DanhMucs
                    .OrderBy(x => x.TenDanhMuc)
                    .ToListAsync(),
                "MaDanhMuc",
                "TenDanhMuc",
                p?.MaDanhMuc
            );

            ViewBag.KhuVucHienThiId = new SelectList(
                await _context.KhuVucHienThis
                    .OrderBy(x => x.Ten)
                    .ToListAsync(),
                "Id",
                "Ten",
                p?.KhuVucHienThiId
            );
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "products"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception("Upload ảnh lên Cloudinary thất bại: " + uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl.ToString();
        }

        private void DeleteImageFile(string? imagePath)
        {
            // Tạm thời không xóa ảnh trên Cloudinary để tránh lỗi.
        }
    }
}