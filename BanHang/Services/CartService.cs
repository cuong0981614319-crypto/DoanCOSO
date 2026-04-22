using BanHang.Extensions;
using BanHang.Models;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private const string CartKey = "CART";

    public CartService(ApplicationDbContext context)
    {
        _context = context;
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
        var sp = await _context.SanPhams.FindAsync(productId);
        if (sp == null) return false;

        // 2. Lấy giỏ hàng hiện tại từ Session
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(c => c.MaSanPham == productId);

        if (item == null)
        {
            // 3. Nếu chưa có, thêm mới và PHẢI GÁN GiaKhuyenMai ở đây
            cart.Add(new CartItem
            {
                MaSanPham = sp.MaSanPham,
                TenSanPham = sp.TenSanPham,
                Gia = sp.Gia,

                // ĐÂY LÀ DÒNG QUAN TRỌNG NHẤT:
                // Nó sẽ lấy logic (DaBan < 10 thì giảm 20%) từ model SanPham qua
                GiaKhuyenMai = sp.GiaKhuyenMai,

                SoLuong = quantity,
                HinhAnh = sp.HinhAnh
            });
        }
        else
        {
            // 4. Nếu có rồi thì tăng số lượng
            item.SoLuong += quantity;

            // Cập nhật lại giá khuyến mãi (phòng trường hợp logic DaBan thay đổi)
            item.GiaKhuyenMai = sp.GiaKhuyenMai;
        }

        // 5. Lưu lại vào Session
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
        {
            cart.Remove(item);
        }
        else
        {
            item.SoLuong = quantity;
        }

        SaveCart(session, cart);
    }
}