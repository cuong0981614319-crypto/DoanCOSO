using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TenNguoiDung { get; set; } // Hoặc dùng UserId nếu đã đăng nhập

        [Required]
        [Range(1, 5)]
        public int Diem { get; set; } // Từ 1 đến 5 sao

        [Required]
        public string NoiDung { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết tới sản phẩm
        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public SanPham? SanPham { get; set; }
        public List<DanhGiaImage> Images { get; set; }
    }
}