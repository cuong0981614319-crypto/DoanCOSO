using BanHang.Models;

namespace BanHang.Services
{
    public interface IThongKeService
    {
        Task<ThongKeDoanhThuViewModel> GetThongKeAsync(DateTime? fromDate, DateTime? toDate);
    }
}
