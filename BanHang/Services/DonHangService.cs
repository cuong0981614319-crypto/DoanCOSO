using BanHang.Models;
using BanHang.Repositories;

namespace BanHang.Services
{
    public class DonHangService : IDonHangService
    {
        private readonly IDonHangRepository _repo;

        public DonHangService(IDonHangRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DonHang>> GetMyOrdersAsync(string userId)
        {
            var donHangs = await _repo.GetByUserIdAsync(userId);
            // Chuyển sang giờ Việt Nam (UTC+7)
            donHangs.ForEach(x => x.NgayDat = x.NgayDat.AddHours(7));
            return donHangs;
        }

        public async Task<DonHang?> GetOrderDetailsAsync(int id, string userId)
        {
            return await _repo.GetByIdForUserAsync(id, userId);
        }

        public async Task<(bool success, string error)> CancelOrderAsync(int id, string userId)
        {
            var donHang = await _repo.GetByIdWithDetailsAsync(id);

            if (donHang == null)
                return (false, "Không tìm thấy đơn hàng.");

            // ===== Business logic nằm ở đây — chỉ một chỗ duy nhất =====
            var cancelableStatuses = new[] { "Chờ xác nhận", "Chờ thanh toán" };
            if (!cancelableStatuses.Contains(donHang.TrangThai))
                return (false, "Đơn hàng không thể hủy.");

            if (donHang.DaThanhToan)
                return (false, "Đơn đã thanh toán, không thể hủy.");
            // ================================================================

            await _repo.SetStatusAsync(id, "Đã hủy");
            return (true, string.Empty);
        }

        public async Task<(bool success, string error)> SubmitDanhGiaAsync(
            int sanPhamId, int diem, string noiDung, string userName)
        {
            if (diem < 1 || diem > 5)
                return (false, "Điểm đánh giá phải từ 1-5 sao!");

            if (string.IsNullOrWhiteSpace(noiDung) || noiDung.Length < 10)
                return (false, "Nhận xét phải có ít nhất 10 ký tự!");

            if (noiDung.Length > 500)
                return (false, "Nhận xét không được quá 500 ký tự!");

            if (await _repo.DaDanhGiaAsync(sanPhamId, userName))
                return (false, "Bạn đã đánh giá sản phẩm này rồi!");

            var danhGia = new DanhGia
            {
                SanPhamId = sanPhamId,
                TenNguoiDung = userName,
                Diem = diem,
                NoiDung = noiDung.Trim(),
                NgayTao = DateTime.Now
            };

            await _repo.AddDanhGiaAsync(danhGia);
            return (true, string.Empty);
        }
    }
}
