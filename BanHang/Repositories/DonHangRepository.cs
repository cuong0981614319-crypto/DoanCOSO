using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class DonHangRepository : IDonHangRepository
    {
        private readonly ApplicationDbContext _context;

        public DonHangRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DonHang>> GetByUserIdAsync(string userId)
        {
            return await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();
        }

        public async Task<DonHang?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Include(x => x.LichSuDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == id && x.UserId == userId);
        }

        public async Task<DonHang?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == id);
        }

        /// <summary>Chỉ cập nhật trạng thái — mọi kiểm tra nghiệp vụ phải được làm ở Service trước khi gọi hàm này.</summary>
        public async Task SetStatusAsync(int id, string trangThai)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return;
            donHang.TrangThai = trangThai;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddDanhGiaAsync(DanhGia danhGia)
        {
            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DaDanhGiaAsync(int sanPhamId, string tenNguoiDung)
        {
            return await _context.DanhGias
                .AnyAsync(d => d.SanPhamId == sanPhamId && d.TenNguoiDung == tenNguoiDung);
        }
    }
}
