using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class ThanhToan
    {
        [Required]
        public string HoTen { get; set; } = "";

        [Required]
        public string SoDienThoai { get; set; } = "";

        [Required]
        public string DiaChi { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PhuongThucThanhToan { get; set; } = "";

        public string? GhiChu { get; set; }
    }
}