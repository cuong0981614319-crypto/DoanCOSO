using BanHang.Models;
using BanHang.Services.Interfaces;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace BanHang.Services
{
    public class AdminProductService : IAdminProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        public AdminProductService(ApplicationDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<List<SanPham>> GetAllAsync()
        {
            return await _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.KhuVucHienThi)
                .OrderByDescending(x => x.MaSanPham)
                .ToListAsync();
        }

        public async Task<SanPham?> GetByIdAsync(int id)
        {
            return await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);
        }

        public async Task<bool> CreateAsync(SanPham model, ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) return false;

            var sanPham = new SanPham
            {
                TenSanPham = model.TenSanPham,
                Gia = model.Gia,
                MoTa = model.MoTa,
                MauSac = model.MauSac,
                MaDanhMuc = model.MaDanhMuc,
                KhuVucHienThiId = model.KhuVucHienThiId,
                kichthuc = model.kichthuc,
                chatlieu = model.chatlieu,
                HinhAnhSanPhams = new List<HinhAnhSanPham>()
            };

            // 🔥 Upload nhiều ảnh
            if (model.ImageFiles != null && model.ImageFiles.Any(f => f.Length > 0))
            {
                bool isFirst = true;

                foreach (var file in model.ImageFiles.Where(f => f.Length > 0))
                {
                    var url = await UploadImageAsync(file);

                    if (isFirst)
                    {
                        sanPham.HinhAnh = url; // ảnh chính
                        isFirst = false;
                    }

                    sanPham.HinhAnhSanPhams.Add(new HinhAnhSanPham
                    {
                        DuongDanAnh = url
                    });
                }
            }

            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(int id, SanPham model, ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) return false;

            var existing = await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (existing == null) return false;

            // Update info
            existing.TenSanPham = model.TenSanPham;
            existing.Gia = model.Gia;
            existing.MoTa = model.MoTa;
            existing.MauSac = model.MauSac;
            existing.MaDanhMuc = model.MaDanhMuc;
            existing.KhuVucHienThiId = model.KhuVucHienThiId;
            existing.kichthuc = model.kichthuc;
            existing.chatlieu = model.chatlieu;

            // 🔥 XÓA ảnh
            if (model.DeletedImageIds != null && model.DeletedImageIds.Any())
            {
                var images = existing.HinhAnhSanPhams
                    .Where(x => model.DeletedImageIds.Contains(x.Id))
                    .ToList();

                foreach (var img in images)
                {
                    _context.HinhAnhSanPhams.Remove(img);
                }
            }

            // 🔥 THÊM ảnh mới
            if (model.ImageFiles != null && model.ImageFiles.Any(f => f.Length > 0))
            {
                foreach (var file in model.ImageFiles.Where(f => f.Length > 0))
                {
                    var url = await UploadImageAsync(file);

                    _context.HinhAnhSanPhams.Add(new HinhAnhSanPham
                    {
                        MaSanPham = existing.MaSanPham,
                        DuongDanAnh = url
                    });
                }
            }

            await _context.SaveChangesAsync();

            // 🔥 SET thumbnail
            if (!string.IsNullOrEmpty(model.SelectedThumbnail))
            {
                existing.HinhAnh = model.SelectedThumbnail;
            }
            else
            {
                existing.HinhAnh = existing.HinhAnhSanPhams
                    .FirstOrDefault()?.DuongDanAnh;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return false;

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();
            return true;
        }



        public async Task LoadDropdownsAsync(dynamic viewBag, SanPham? model = null)
        {
            viewBag.MaDanhMuc = new SelectList(
                await _context.DanhMucs.ToListAsync(),
                "MaDanhMuc",
                "TenDanhMuc",
                model?.MaDanhMuc
            );

            viewBag.KhuVucHienThiId = new SelectList(
                await _context.KhuVucHienThis.ToListAsync(),
                "Id",
                "Ten",
                model?.KhuVucHienThiId
            );
        }
        
    private async Task<string> UploadImageAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "products"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception("Upload ảnh thất bại: " + result.Error.Message);
            }

            return result.SecureUrl.ToString();
        }

    }
}