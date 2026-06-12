using BanHang.Extensions;
using BanHang.Models;

public class CartService : ICartService
{
    private readonly IProductRepository _productRepo;
    private const string CartKey = "CART";

    public CartService(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public List<CartItem> GetCart(ISession session)
    {
        return session.GetObjectFromJson<List<CartItem>>(CartKey) ?? new List<CartItem>();
    }

    public void SaveCart(ISession session, List<CartItem> cart)
    {
        session.SetObjectAsJson(CartKey, cart);
    }

    public async Task<bool> AddToCart(ISession session, int productId, int quantity)
    {
        var sp = await _productRepo.GetById(productId);
        if (sp == null) return false;

        var cart = GetCart(session);
        var item = cart.FirstOrDefault(c => c.MaSanPham == productId);

        if (item == null)
        {
            cart.Add(new CartItem
            {
                MaSanPham    = sp.MaSanPham,
                TenSanPham   = sp.TenSanPham,
                Gia          = sp.Gia,
                GiaKhuyenMai = sp.GiaKhuyenMai,
                SoLuong      = quantity,
                HinhAnh      = sp.HinhAnh
            });
        }
        else
        {
            item.SoLuong      += quantity;
            item.GiaKhuyenMai  = sp.GiaKhuyenMai;
        }

        SaveCart(session, cart);
        return true;
    }

    public void Remove(ISession session, int productId)
    {
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(x => x.MaSanPham == productId);
        if (item != null)
        {
            cart.Remove(item);
            SaveCart(session, cart);
        }
    }

    public void Clear(ISession session)
    {
        session.Remove(CartKey);
    }

    public void UpdateQuantity(ISession session, int id, int quantity)
    {
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(x => x.MaSanPham == id);
        if (item == null) return;

        if (quantity <= 0)
            cart.Remove(item);
        else
            item.SoLuong = quantity;

        SaveCart(session, cart);
    }
}