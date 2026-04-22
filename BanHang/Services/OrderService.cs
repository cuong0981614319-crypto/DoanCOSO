using BanHang.Models;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DonHang> CreateOrder(string userId, ThanhToan model, List<CartItem> cart)
    {
        // 1. Tạo đối tượng đơn hàng cha
        var donHang = new DonHang
        {
            UserId = userId,
            HoTen = model.HoTen,
            SoDienThoai = model.SoDienThoai,
            DiaChi = model.DiaChi,
            GhiChu = model.GhiChu, // Đã thêm ghi chú ở đây
            PhuongThucThanhToan = model.PhuongThucThanhToan,
            NgayDat = DateTime.Now,
            TongTien = cart.Sum(x => x.ThanhTien),
            TrangThai = "Chờ xác nhận",
            DaThanhToan = false,
            // Khởi tạo danh sách chi tiết trống bên trong đơn hàng
            ChiTietDonHangs = new List<ChiTietDonHang>()
        };

        // 2. Duyệt giỏ hàng và thêm trực tiếp vào List ChiTietDonHangs của đối tượng donHang
        foreach (var item in cart)
        {
            var chiTiet = new ChiTietDonHang
            {
                MaSanPham = item.MaSanPham,
                SoLuong = item.SoLuong,
                // Ưu tiên lấy giá khuyến mãi nếu có, không thì lấy giá gốc
                DonGia = item.GiaKhuyenMai > 0 ? item.GiaKhuyenMai : item.Gia
            };

            // Thêm vào list của object cha (EF Core sẽ tự động quản lý khóa ngoại)
            donHang.ChiTietDonHangs.Add(chiTiet);
        }

        // 3. Chỉ cần Add cái 'donHang' cha vào Context
        // EF Core sẽ tự hiểu là phải lưu Đơn hàng trước để lấy ID, rồi mới lưu Chi tiết
        _context.DonHangs.Add(donHang);

        // 4. Lưu tất cả thay đổi trong 1 lần duy nhất
        await _context.SaveChangesAsync();

        return donHang;
    }
}