using BanHang.Models;
using BanHang.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _context;

        public HomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<KhuVucHienThi>> GetKhuVucsWithSanPhamsAsync()
        {
            return await _context.KhuVucHienThis
                .Include(k => k.SanPhams)
                    .ThenInclude(sp => sp.DanhMuc)
                .OrderBy(k => k.ThuTu)
                .ToListAsync();
        }

        public async Task<DanhMuc?> GetDanhMucByIdAsync(int maDanhMuc)
        {
            return await _context.DanhMucs
                .FirstOrDefaultAsync(x => x.MaDanhMuc == maDanhMuc);
        }

        public async Task<List<SanPham>> GetBestSellersAsync(int minDaBan, int take)
        {
            return await _context.SanPhams
                .Where(sp => sp.DaBan > minDaBan)
                .OrderByDescending(sp => sp.DaBan)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<RatingResult>> GetAllRatingsAsync()
        {
            return await _context.DanhGias
                .GroupBy(d => d.SanPhamId)
                .Select(g => new RatingResult
                {
                    SanPhamId = g.Key,
                    Avg = g.Average(x => x.Diem),
                    Count = g.Count()
                })
                .ToListAsync();
        }

        public async Task<List<SanPham>> SearchSanPhamsAsync(string keyword, int take)
        {
            return await _context.SanPhams
                .Where(x => x.TenSanPham != null && x.TenSanPham.Contains(keyword))
                .OrderBy(x => x.TenSanPham)
                .Take(take)
                .ToListAsync();
        }
    }
}
