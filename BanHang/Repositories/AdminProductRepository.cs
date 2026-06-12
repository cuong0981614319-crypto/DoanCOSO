using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class AdminProductRepository : IAdminProductRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SanPham>> GetAllWithRelationsAsync()
        {
            return await _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.KhuVucHienThi)
                .OrderByDescending(x => x.MaSanPham)
                .ToListAsync();
        }

        public async Task<SanPham?> GetByIdWithImagesAsync(int id)
        {
            return await _context.SanPhams
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);
        }

        public async Task<SanPham?> GetByIdAsync(int id)
        {
            return await _context.SanPhams.FindAsync(id);
        }

        public async Task AddAsync(SanPham sanPham)
        {
            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SanPham sanPham)
        {
            _context.SanPhams.Update(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveImageAsync(HinhAnhSanPham image)
        {
            _context.HinhAnhSanPhams.Remove(image);
            await _context.SaveChangesAsync();
        }

        public async Task AddImageAsync(HinhAnhSanPham image)
        {
            _context.HinhAnhSanPhams.Add(image);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SanPham sanPham)
        {
            _context.SanPhams.Remove(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DanhMuc>> GetDanhMucsAsync()
        {
            return await _context.DanhMucs.ToListAsync();
        }

        public async Task<List<KhuVucHienThi>> GetKhuVucHienThisAsync()
        {
            return await _context.KhuVucHienThis.ToListAsync();
        }
    }
}
