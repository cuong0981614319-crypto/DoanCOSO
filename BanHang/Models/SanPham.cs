using BanHang.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class SanPham
    {
        [Key]
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string TenSanPham { get; set; } = string.Empty; // Thêm = string.Empty để hết warning

        public decimal Gia { get; set; }

        public string? MoTa { get; set; } // Thêm dấu ? để chấp nhận null

        public string? HinhAnh { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public int SoLuong { get; set; }

        public int MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; } // Thêm virtual để hỗ trợ Lazy Loading nếu cần
    }
}