using BanHang.Models;

namespace BanHang.Services
{
    public interface IAdminDonHangService
    {
        Task<List<DonHang>> GetAllAsync(string? status);
        Task<DonHang?> GetByIdWithDetailsAsync(int id);
        Task<bool> UpdateStatusAsync(int maDonHang, string trangThai);
        Task<bool> DeleteAsync(int id);
    }
}
