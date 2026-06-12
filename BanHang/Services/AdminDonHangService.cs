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
            if (donHang == null) return false;

            // ===== Business logic nằm đúng chỗ trong Service =====
            if (trangThai == "Hoàn thành" && donHang.TrangThai != "Hoàn thành")
            {
                donHang.DaThanhToan    = true;
                donHang.NgayThanhToan  = DateTime.Now;

                // Tăng DaBan cho từng sản phẩm trong đơn
                await _repo.IncrementDaBanAsync(donHang.ChiTietDonHangs);
            }
            // ====================================================

            donHang.TrangThai = trangThai;
            await _repo.UpdateAsync(donHang);
            return true;
        }

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}
