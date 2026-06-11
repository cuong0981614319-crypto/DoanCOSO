using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IOrderRepository
    {
        Task<DonHang> CreateAsync(DonHang donHang);
        Task<DonHang?> GetByIdAsync(int id);
        Task UpdatePaymentAsync(int id, bool daThanhToan, DateTime? ngayThanhToan);
    }
}
