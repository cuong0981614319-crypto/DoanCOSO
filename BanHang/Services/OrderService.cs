using BanHang.Models;
using BanHang.Repositories;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;

    public OrderService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<DonHang> CreateOrder(string userId, ThanhToan model, List<CartItem> cart)
    {
        var donHang = new DonHang
        {
            UserId = userId,
            HoTen = model.HoTen,
            SoDienThoai = model.SoDienThoai,
            DiaChi = model.DiaChi,
            GhiChu = model.GhiChu,
            PhuongThucThanhToan = model.PhuongThucThanhToan,
            NgayDat = DateTime.Now,
            TongTien = cart.Sum(x => x.ThanhTien),
            TrangThai = "Chờ xác nhận",
            DaThanhToan = false,
            ChiTietDonHangs = cart.Select(item => new ChiTietDonHang
            {
                MaSanPham = item.MaSanPham,
                SoLuong = item.SoLuong,
                DonGia = item.GiaKhuyenMai > 0 ? item.GiaKhuyenMai : item.Gia
            }).ToList()
        };

        return await _repo.CreateAsync(donHang);
    }
}