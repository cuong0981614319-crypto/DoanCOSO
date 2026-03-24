namespace BanHang.Models
{
    public class CartItem
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; } = "";
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public string? HinhAnh { get; set; }

        public decimal ThanhTien => Gia * SoLuong;
    }
}