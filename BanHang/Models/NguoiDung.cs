using System.ComponentModel.DataAnnotations;
namespace BanHang.Models
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required]
        public string TenNguoiDung { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string MatKhau { get; set; }

        public string DienThoai { get; set; }

        public string DiaChi { get; set; }
    }
}