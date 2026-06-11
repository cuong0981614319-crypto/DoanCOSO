using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class DanhMucRepository : IDanhMucRepository
    {
        private readonly ApplicationDbContext _context;

        public DanhMucRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DanhMuc>> GetAllAsync()
        {
            return await _context.DanhMucs
                .OrderBy(x => x.MaDanhMuc)
                .ToListAsync();
        }

        public async Task<DanhMuc?> GetByIdAsync(int id)
        {
            return await _context.DanhMucs
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.MaDanhMuc == id);
        }

        public async Task AddAsync(DanhMuc danhMuc)
        {
            _context.DanhMucs.Add(danhMuc);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DanhMuc danhMuc)
        {
            var existing = await _context.DanhMucs.FindAsync(danhMuc.MaDanhMuc);
            if (existing == null) return;
            existing.TenDanhMuc = danhMuc.TenDanhMuc;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var danhMuc = await _context.DanhMucs
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.MaDanhMuc == id);

            if (danhMuc == null) return false;
            if (danhMuc.SanPhams != null && danhMuc.SanPhams.Any()) return false;

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
