using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BanHang.Models
{
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }

        public int MaDonHang { get; set; }

        public string PhuongThuc { get; set; }

        public string TrangThai { get; set; }

        [ForeignKey("MaDonHang")]
        public DonHang DonHang { get; set; }
    }
}