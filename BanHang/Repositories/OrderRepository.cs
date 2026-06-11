using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DonHang> CreateAsync(DonHang donHang)
        {
            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();
            return donHang;
        }

        public async Task<DonHang?> GetByIdAsync(int id)
        {
            return await _context.DonHangs.FindAsync(id);
        }

        public async Task UpdatePaymentAsync(int id, bool daThanhToan, DateTime? ngayThanhToan)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return;
            donHang.DaThanhToan = daThanhToan;
            donHang.NgayThanhToan = ngayThanhToan;
            await _context.SaveChangesAsync();
        }
    }
}
