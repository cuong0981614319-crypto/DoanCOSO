using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }

        public string HoTenNguoiNhan { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string DiaChiGiaoHang { get; set; } = "";
        public string? GhiChu { get; set; }

        public ICollection<ChiTietDonHang>? ChiTietDonHangs { get; set; }
        public string PhuongThucThanhToan { get; set; } = "";
        public string TrangThai { get; set; } = "Chờ xử lý";
        public string Email { get; set; } = "";
    }
}