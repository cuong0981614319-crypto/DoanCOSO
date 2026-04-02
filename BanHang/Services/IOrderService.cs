using BanHang.Models;

public interface IOrderService
{
    Task<DonHang> CreateOrder(string userId, ThanhToan model, List<CartItem> cart);
}