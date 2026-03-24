using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class KhuVucHienThi
    {
        public int Id { get; set; }

        [Required]
        public string Ten { get; set; }

        public int ThuTu { get; set; } // 👈 THÊM DÒNG NÀY

        public ICollection<SanPham>? SanPhams { get; set; }
    }
}