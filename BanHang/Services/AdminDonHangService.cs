using BanHang.Models;
using BanHang.Repositories;

namespace BanHang.Services
{
    public class AdminDonHangService : IAdminDonHangService
    {
        private readonly IAdminDonHangRepository _repo;

        public AdminDonHangService(IAdminDonHangRepository repo)
        {
            _repo = repo;
        }

        public Task<List<DonHang>> GetAllAsync(string? status)
            => _repo.GetAllAsync(status);

        public Task<DonHang?> GetByIdWithDetailsAsync(int id)
            => _repo.GetByIdWithDetailsAsync(id);

        public async Task<bool> UpdateStatusAsync(int maDonHang, string trangThai)
        {
            var donHang = await _repo.GetByIdWithDetailsAsync(maDonHang);

            if (donHang == null)
                return false;

            if (trangThai == "Hoàn thành" && donHang.TrangThai != "Hoàn thành")
            {
                donHang.DaThanhToan = true;
                donHang.NgayThanhToan = DateTime.Now;

                await _repo.IncrementDaBanAsync(donHang.ChiTietDonHangs);
            }

            // Lưu trạng thái cũ nếu muốn
            var trangThaiCu = donHang.TrangThai;

            donHang.TrangThai = trangThai;

            // THÊM LỊCH SỬ
            donHang.LichSuDonHangs.Add(new LichSuDonHang
            {
                TrangThaiMoi = trangThai,
                GhiChu = $"Đổi từ '{trangThaiCu}' sang '{trangThai}'",
                NgayTao = DateTime.Now
            });

            await _repo.UpdateAsync(donHang);

            return true;
        }
        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}
