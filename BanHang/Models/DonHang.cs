using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        public DateTime NgayDat { get; set; }

        public int MaNguoiDung { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; }

        [ForeignKey("MaNguoiDung")]
        public NguoiDung NguoiDung { get; set; }
    }
}
