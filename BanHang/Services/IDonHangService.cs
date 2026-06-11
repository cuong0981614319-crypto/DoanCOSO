using BanHang.Models;

namespace BanHang.Services
{
    public interface IDonHangService
    {
        Task<List<DonHang>> GetMyOrdersAsync(string userId);
        Task<DonHang?> GetOrderDetailsAsync(int id, string userId);
        Task<(bool success, string error)> CancelOrderAsync(int id, string userId);
        Task<(bool success, string error)> SubmitDanhGiaAsync(int sanPhamId, int diem, string noiDung, string userName);
    }
}
