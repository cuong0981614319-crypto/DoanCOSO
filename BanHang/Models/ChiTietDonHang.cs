using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaDonHang { get; set; }

        public int MaSanPham { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("MaSanPham")]
        public virtual SanPham? SanPham { get; set; }
    }
}