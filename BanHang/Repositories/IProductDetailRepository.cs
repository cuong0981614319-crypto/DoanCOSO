using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IProductDetailRepository
    {
        Task<SanPham?> GetWithDetailsAsync(int id);
        Task<List<SanPham>> GetSameCategoryAsync(int sanPhamId, int maDanhMuc, int take);
        Task<double> GetAvgRatingAsync(int sanPhamId);
        Task<int> GetTotalReviewsAsync(int sanPhamId);
        Task<(List<DanhGia> items, int total)> GetReviewsPagedAsync(int sanPhamId, int? star, bool? hasImage, int page, int pageSize);
        Task AddReviewAsync(DanhGia danhGia, List<IFormFile> images, string uploadPath);
        Task<SanPham?> GetBasicAsync(int id);
        Task<List<string>> GetDistinctKichThucsAsync();
        Task<List<string>> GetDistinctMauSacsAsync();
        Task<List<DanhMuc>> GetAllDanhMucsAsync();
    }
}
