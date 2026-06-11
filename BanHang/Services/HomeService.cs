using BanHang.Models;
using BanHang.Repositories;

namespace BanHang.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _homeRepo;
        private readonly IProductRepository _productRepo;

        public HomeService(IHomeRepository homeRepo, IProductRepository productRepo)
        {
            _homeRepo = homeRepo;
            _productRepo = productRepo;
        }

        public async Task<IEnumerable<SanPham>> GetPromoProductsAsync()
        {
            return await _productRepo.GetProductsNeverSoldAsync();
        }

        public async Task<(List<KhuVucHienThi> khuVucs, List<SanPham> bestSellers)> GetHomeDataAsync(
            int? maDanhMuc, string? keyword)
        {
            var khuVucs = await _homeRepo.GetKhuVucsWithSanPhamsAsync();

            // Lọc theo danh mục
            if (maDanhMuc.HasValue)
            {
                foreach (var kv in khuVucs)
                    kv.SanPhams = kv.SanPhams
                        .Where(sp => sp.MaDanhMuc == maDanhMuc.Value)
                        .ToList();

                khuVucs = khuVucs.Where(kv => kv.SanPhams.Any()).ToList();
            }

            // Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                foreach (var kv in khuVucs)
                    kv.SanPhams = kv.SanPhams
                        .Where(sp => sp.TenSanPham != null && sp.TenSanPham.ToLower().Contains(kw))
                        .ToList();

                khuVucs = khuVucs.Where(kv => kv.SanPhams.Any()).ToList();
            }

            // Lấy rating và gán vào sản phẩm
            var ratings = await _homeRepo.GetAllRatingsAsync();
            var allProducts = khuVucs.SelectMany(kv => kv.SanPhams).ToList();

            var bestSellers = await _homeRepo.GetBestSellersAsync(5, 10);

            foreach (var sp in allProducts.Concat(bestSellers))
            {
                var r = ratings.FirstOrDefault(x => x.SanPhamId == sp.MaSanPham);
                sp.AvgRating = r?.Avg ?? 0;
                sp.TotalReviews = r?.Count ?? 0;
            }

            return (khuVucs, bestSellers);
        }

        public async Task<List<object>> SearchSuggestionsAsync(string keyword, Func<int, string?> urlBuilder)
        {
            var products = await _homeRepo.SearchSanPhamsAsync(keyword.Trim(), 8);

            return products.Select(x => (object)new
            {
                id = x.MaSanPham,
                name = x.TenSanPham,
                price = x.Gia,
                image = x.HinhAnh,
                url = urlBuilder(x.MaSanPham)
            }).ToList();
        }
    }
}
