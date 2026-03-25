namespace BanHang.Models
{
    public class HinhAnhSanPham
    {
        public int Id { get; set; }

        public int MaSanPham { get; set; }
        public SanPham? SanPham { get; set; }

        public string DuongDanAnh { get; set; } = string.Empty;
    }
}