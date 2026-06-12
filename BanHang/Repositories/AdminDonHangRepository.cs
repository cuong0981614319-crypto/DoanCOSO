using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class AdminDonHangRepository : IAdminDonHangRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminDonHangRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DonHang>> GetAllAsync(string? status)
        {
            var query = _context.DonHangs.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Đã Thanh Toán")
                    query = query.Where(x => x.DaThanhToan);
                else if (status == "Chờ thanh toán")
                    query = query.Where(x => !x.DaThanhToan);
                else
                    query = query.Where(x => x.TrangThai == status);
            }

            return await query
                .Include(x => x.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();
        }

        public async Task<DonHang?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Include(x => x.LichSuDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == id);
        }

        /// <summary>Chỉ lưu entity đã được Service chỉnh sửa — không có business logic ở đây.</summary>
        public async Task UpdateAsync(DonHang donHang)
        {
            _context.DonHangs.Update(donHang);
            await _context.SaveChangesAsync();
        }

        /// <summary>Tăng DaBan cho từng sản phẩm — được gọi bởi Service khi đơn Hoàn thành.</summary>
        public async Task IncrementDaBanAsync(IEnumerable<ChiTietDonHang> chiTiets)
        {
            foreach (var item in chiTiets)
            {
                var sanPham = await _context.SanPhams.FindAsync(item.MaSanPham);
                if (sanPham != null)
                    sanPham.DaBan += item.SoLuong;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == id);

            if (donHang == null) return false;

            if (donHang.ChiTietDonHangs.Any())
                _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);

            _context.DonHangs.Remove(donHang);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
