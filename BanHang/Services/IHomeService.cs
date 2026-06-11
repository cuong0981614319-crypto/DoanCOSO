using BanHang.Models;
using BanHang.Repositories;

namespace BanHang.Services
{
    public interface IHomeService
    {
        Task<(List<KhuVucHienThi> khuVucs, List<SanPham> bestSellers)> GetHomeDataAsync(int? maDanhMuc, string? keyword);
        Task<List<object>> SearchSuggestionsAsync(string keyword, Func<int, string?> urlBuilder);
        Task<IEnumerable<SanPham>> GetPromoProductsAsync();
    }
}
