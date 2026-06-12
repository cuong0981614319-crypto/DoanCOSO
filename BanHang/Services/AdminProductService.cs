using BanHang.Models;
using BanHang.Services.Interfaces;
using BanHang.Repositories;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using CloudinaryDotNet.Actions;

namespace BanHang.Services
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IAdminProductRepository _repo;
        private readonly Cloudinary _cloudinary;

        public AdminProductService(IAdminProductRepository repo, Cloudinary cloudinary)
        {
            _repo = repo;
            _cloudinary = cloudinary;
        }

        public async Task<List<SanPham>> GetAllAsync()
        {
            return await _repo.GetAllWithRelationsAsync();
        }

        public async Task<SanPham?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdWithImagesAsync(id);
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
                HinhAnhSanPhams = new List<HinhAnhSanPham>(),
                NgayThem = model.NgayThem,
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

            await _repo.AddAsync(sanPham);
            return true;
        }

        public async Task<bool> UpdateAsync(int id, SanPham model, ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) return false;

            var existing = await _repo.GetByIdWithImagesAsync(id);
            if (existing == null) return false;

            // Cập nhật thông tin cơ bản
            existing.TenSanPham = model.TenSanPham;
            existing.Gia = model.Gia;
            existing.MoTa = model.MoTa;
            existing.MauSac = model.MauSac;
            existing.MaDanhMuc = model.MaDanhMuc;
            existing.KhuVucHienThiId = model.KhuVucHienThiId;
            existing.NgayThem = model.NgayThem;
            existing.kichthuc = model.kichthuc;
            existing.chatlieu = model.chatlieu;

            // 🔥 XÓA ảnh cũ được chọn
            if (model.DeletedImageIds != null && model.DeletedImageIds.Any())
            {
                var images = existing.HinhAnhSanPhams
                    .Where(x => model.DeletedImageIds.Contains(x.Id))
                    .ToList();

                foreach (var img in images)
                {
                    await _repo.RemoveImageAsync(img);
                }
            }

            // 🔥 THÊM ảnh mới
            if (model.ImageFiles != null && model.ImageFiles.Any(f => f.Length > 0))
            {
                foreach (var file in model.ImageFiles.Where(f => f.Length > 0))
                {
                    var url = await UploadImageAsync(file);

                    await _repo.AddImageAsync(new HinhAnhSanPham
                    {
                        MaSanPham = existing.MaSanPham,
                        DuongDanAnh = url
                    });
                }
            }

            await _repo.UpdateAsync(existing);

            // 🔥 SET thumbnail đại diện
            if (!string.IsNullOrEmpty(model.SelectedThumbnail))
            {
                existing.HinhAnh = model.SelectedThumbnail;
            }
            else
            {
                existing.HinhAnh = existing.HinhAnhSanPhams
                    .FirstOrDefault()?.DuongDanAnh;
            }

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sp = await _repo.GetByIdAsync(id);
            if (sp == null) return false;

            await _repo.DeleteAsync(sp);
            return true;
        }

        public async Task LoadDropdownsAsync(dynamic viewBag, SanPham? model = null)
        {
            viewBag.MaDanhMuc = new SelectList(
                await _repo.GetDanhMucsAsync(),
                "MaDanhMuc",
                "TenDanhMuc",
                model?.MaDanhMuc
            );

            viewBag.KhuVucHienThiId = new SelectList(
                await _repo.GetKhuVucHienThisAsync(),
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
