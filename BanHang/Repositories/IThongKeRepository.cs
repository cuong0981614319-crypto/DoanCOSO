using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IThongKeRepository
    {
        Task<decimal> GetDoanhThuHomNayAsync();
        Task<decimal> GetDoanhThuThangNayAsync();
        Task<decimal> GetTongDoanhThuAsync();
        Task<int> GetSoDonDaThanhToanAsync();
        Task<List<DoanhThuTheoNgayItem>> GetDoanhThuTheoNgayAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<(string Label, int Count)>> GetThongKeDanhMucAsync();
    }
}
