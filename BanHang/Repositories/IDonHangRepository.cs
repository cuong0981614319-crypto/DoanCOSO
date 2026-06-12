using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IDonHangRepository
    {
        Task<List<DonHang>> GetByUserIdAsync(string userId);
        Task<DonHang?> GetByIdForUserAsync(int id, string userId);
        Task<DonHang?> GetByIdWithDetailsAsync(int id);
        Task SetStatusAsync(int id, string trangThai);
        Task<bool> AddDanhGiaAsync(DanhGia danhGia);
        Task<bool> DaDanhGiaAsync(int sanPhamId, string tenNguoiDung);
    }
}
