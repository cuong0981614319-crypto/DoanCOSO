namespace BanHang.Models
{
    public class ThongKeDoanhThuViewModel
    {
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int SoDonDaThanhToan { get; set; }

        public List<DoanhThuTheoNgayItem> DoanhThuTheoNgay { get; set; } = new();
    }

    public class DoanhThuTheoNgayItem
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
    }
}