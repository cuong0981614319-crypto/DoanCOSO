using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class KhuVucRepository : IKhuVucRepository
    {
        private readonly ApplicationDbContext _context;

        public KhuVucRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<KhuVucHienThi>> GetAllAsync()
        {
            return await _context.KhuVucHienThis.ToListAsync();
        }

        public async Task<KhuVucHienThi?> GetByIdWithSanPhamsAsync(int id)
        {
            return await _context.KhuVucHienThis
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(KhuVucHienThi kv)
        {
            _context.Add(kv);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(KhuVucHienThi kv)
        {
            _context.Update(kv);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var khuVuc = await _context.KhuVucHienThis
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (khuVuc == null) return false;
            if (khuVuc.SanPhams != null && khuVuc.SanPhams.Any()) return false;

            _context.KhuVucHienThis.Remove(khuVuc);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
