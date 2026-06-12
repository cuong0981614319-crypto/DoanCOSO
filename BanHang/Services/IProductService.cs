using BanHang.Models;

public interface IProductService
{
    Task<(List<SanPham>, int totalItems)> GetFilteredProducts(
        int? khuVucId,
        int? maDanhMuc,
        string? mucGia,
        string? mauSac,
        string? keyword,
        int page,
        int pageSize);

    Task<SanPham?> GetDetails(int id);

    Task<List<SanPham>> GetRelatedProducts(int productId, int? maDanhMuc);

    Task<List<SanPham>> SearchProducts(string keyword);
}