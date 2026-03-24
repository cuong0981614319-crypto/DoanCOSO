using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string TenDanhMuc { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public virtual ICollection<SanPham>? SanPhams { get; set; }

        public static implicit operator DanhMuc(string v)
        {
            return new DanhMuc { TenDanhMuc = v };
        }
    }
}