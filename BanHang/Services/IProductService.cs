using BanHang.Models;

public interface IProductService
{
    Task<(List<SanPham>, int totalItems)> GetFilteredProducts(
        int? khuVucId,
        int? maDanhMuc,
        string? mucGia,
        string? mauSac,
        int page,
        int pageSize);

    Task<SanPham?> GetDetails(int id); // ✔ giữ lại 1 cái thôi

    Task<List<SanPham>> GetRelatedProducts(int productId, int? maDanhMuc);
}