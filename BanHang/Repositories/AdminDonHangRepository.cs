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

        public async Task<bool> UpdateStatusAsync(int maDonHang, string trangThai)
        {
            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.MaDonHang == maDonHang);

            if (donHang == null) return false;

            if (trangThai == "Hoàn thành" && donHang.TrangThai != "Hoàn thành")
            {
                donHang.DaThanhToan = true;
                donHang.NgayThanhToan = DateTime.Now;

                foreach (var item in donHang.ChiTietDonHangs)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.MaSanPham);
                    if (sanPham != null)
                    {
                        sanPham.DaBan += item.SoLuong;
                        _context.SanPhams.Update(sanPham);
                    }
                }
            }

            donHang.TrangThai = trangThai;

            _context.lichSuDonHangs.Add(new LichSuDonHang
            {
                DonHang = donHang,
                TrangThaiMoi = trangThai,
                NgayTao = DateTime.Now,
                GhiChu = "Admin cập nhật trạng thái"
            });

            await _context.SaveChangesAsync();
            return true;
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
