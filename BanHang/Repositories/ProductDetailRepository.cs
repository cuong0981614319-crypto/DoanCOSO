using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class ProductDetailRepository : IProductDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SanPham?> GetWithDetailsAsync(int id)
        {
            return await _context.SanPhams
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPhams)
                .FirstOrDefaultAsync(p => p.MaSanPham == id);
        }

        public async Task<List<SanPham>> GetSameCategoryAsync(int sanPhamId, int maDanhMuc, int take)
        {
            return await _context.SanPhams
                .Where(s => s.MaDanhMuc == maDanhMuc && s.MaSanPham != sanPhamId)
                .Take(take)
                .ToListAsync();
        }

        public async Task<double> GetAvgRatingAsync(int sanPhamId)
        {
            return await _context.DanhGias
                .Where(d => d.SanPhamId == sanPhamId)
                .AverageAsync(d => (double?)d.Diem) ?? 0;
        }

        public async Task<int> GetTotalReviewsAsync(int sanPhamId)
        {
            return await _context.DanhGias.CountAsync(d => d.SanPhamId == sanPhamId);
        }

        public async Task<(List<DanhGia> items, int total)> GetReviewsPagedAsync(
            int sanPhamId, int? star, bool? hasImage, int page, int pageSize)
        {
            var query = _context.DanhGias
                .Where(d => d.SanPhamId == sanPhamId)
                .Include(d => d.Images)
                .AsQueryable();

            if (star.HasValue && star > 0)
                query = query.Where(d => d.Diem == star);

            if (hasImage == true)
                query = query.Where(d => d.Images.Any());

            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task AddReviewAsync(DanhGia danhGia, List<IFormFile> images, string uploadPath)
        {
            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        _context.DanhGiaImages.Add(new DanhGiaImage
                        {
                            DanhGiaId = danhGia.Id,
                            ImageUrl = "/images/reviews/" + fileName
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<SanPham?> GetBasicAsync(int id)
        {
            return await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == id);
        }

        public async Task<List<string>> GetDistinctKichThucsAsync()
        {
            return await _context.SanPhams
                .Select(p => p.kichthuc)
                .Distinct()
                .Where(k => !string.IsNullOrEmpty(k))
                .ToListAsync();
        }

        public async Task<List<string>> GetDistinctMauSacsAsync()
        {
            return await _context.SanPhams
                .Select(p => p.MauSac)
                .Distinct()
                .Where(m => !string.IsNullOrEmpty(m))
                .ToListAsync();
        }

        public async Task<List<DanhMuc>> GetAllDanhMucsAsync()
        {
            return await _context.DanhMucs.ToListAsync();
        }
    }
}
