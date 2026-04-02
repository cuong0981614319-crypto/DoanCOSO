using BanHang.Models;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DonHang> CreateOrder(string userId, ThanhToan model, List<CartItem> cart)
    {
        var donHang = new DonHang
        {
            HoTen = model.HoTen,
            SoDienThoai = model.SoDienThoai,
            DiaChi = model.DiaChi,
            NgayDat = DateTime.Now,
            TongTien = cart.Sum(x => x.ThanhTien),
            UserId = userId
        };

        _context.DonHangs.Add(donHang);
        await _context.SaveChangesAsync();

        foreach (var item in cart)
        {
            _context.ChiTietDonHangs.Add(new ChiTietDonHang
            {
                MaDonHang = donHang.MaDonHang,
                MaSanPham = item.MaSanPham,
                SoLuong = item.SoLuong,
                DonGia = item.Gia
            });
        }

        await _context.SaveChangesAsync();
        return donHang;
    }
}