using BanHang.Models;

public interface ICartService
{
    List<CartItem> GetCart(ISession session);
    void SaveCart(ISession session, List<CartItem> cart);
    Task<bool> AddToCart(ISession session, int productId, int quantity);
    void Remove(ISession session, int productId);
    void Clear(ISession session);
    void UpdateQuantity(ISession session, int id, int quantity);
}