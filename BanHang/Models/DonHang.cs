using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        [Required]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required]
        public string DiaChi { get; set; } = string.Empty;

        public DateTime NgayDat { get; set; } = DateTime.Now;

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } = "Chờ xác nhận";
        public string? UserId { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}