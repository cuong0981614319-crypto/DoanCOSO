using System.ComponentModel.DataAnnotations;
namespace BanHang.Models
{
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        public string TenDanhMuc { get; set; }

        public string MoTa { get; set; }

        public static implicit operator DanhMuc(string v)
        {
            return new DanhMuc { TenDanhMuc = v };
        }
    }
}