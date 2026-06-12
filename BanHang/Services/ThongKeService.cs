using BanHang.Models;
using BanHang.Repositories;

namespace BanHang.Services
{
    public class ThongKeService : IThongKeService
    {
        private readonly IThongKeRepository _repo;

        public ThongKeService(IThongKeRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Tổng hợp dữ liệu từ Repository thành ViewModel.
        /// Đây là việc của Service — Repository chỉ trả raw numbers.
        /// </summary>
        public async Task<ThongKeDoanhThuViewModel> GetThongKeAsync(DateTime? fromDate, DateTime? toDate)
        {
            var model = new ThongKeDoanhThuViewModel
            {
                DoanhThuHomNay     = await _repo.GetDoanhThuHomNayAsync(),
                DoanhThuThangNay   = await _repo.GetDoanhThuThangNayAsync(),
                TongDoanhThu       = await _repo.GetTongDoanhThuAsync(),
                SoDonDaThanhToan   = await _repo.GetSoDonDaThanhToanAsync(),
                DoanhThuTheoNgay   = await _repo.GetDoanhThuTheoNgayAsync(fromDate, toDate)
            };
            return model;
        }
    }
}
