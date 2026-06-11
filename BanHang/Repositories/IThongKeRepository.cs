using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IThongKeRepository
    {
        Task<ThongKeDoanhThuViewModel> GetThongKeAsync();
        Task<List<DoanhThuTheoNgayItem>> GetDoanhThuTheoNgayAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<(string Label, int Count)>> GetThongKeDanhMucAsync();
    }
}
