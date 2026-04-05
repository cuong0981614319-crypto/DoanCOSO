namespace BanHang.Models
{
    public class LichSuDonHang
    {
        public int Id { get; set; }
        public int MaDonHang { get; set; }
        public string TrangThaiMoi { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Liên kết với bảng Đơn hàng
        public virtual DonHang DonHang { get; set; }
    }
}
