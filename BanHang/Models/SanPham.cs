using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class SanPham
    {
        [Key]
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string TenSanPham { get; set; } = string.Empty;

        public decimal Gia { get; set; }

        public string? MoTa { get; set; }

        public string? HinhAnh { get; set; }
        public int DaBan { get; set; } = 0;
        [NotMapped]
        public IFormFile? ImageFile { get; set; }


        // ================= DANH MỤC =================
        public int MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }

        // ================= KHU VỰC HIỂN THỊ =================
        public int? KhuVucHienThiId { get; set; }

        [ForeignKey("KhuVucHienThiId")] // ✅ SỬA Ở ĐÂY
        public virtual KhuVucHienThi? KhuVucHienThi { get; set; }
    }
}