using BanHang.Models;
using BanHang.Repositories;

namespace BanHang.Services
{
    public class ProductDetailService : IProductDetailService
    {
        private readonly IProductDetailRepository _repo;

        public ProductDetailService(IProductDetailRepository repo)
        {
            _repo = repo;
        }

        public async Task<SanPham?> GetDetailsAsync(int id, int page, int? star, bool? hasImage, int pageSize)
        {
            var sanPham = await _repo.GetWithDetailsAsync(id);
            if (sanPham == null) return null;

            sanPham.AvgRating = await _repo.GetAvgRatingAsync(id);
            sanPham.TotalReviews = await _repo.GetTotalReviewsAsync(id);

            var (reviews, _) = await _repo.GetReviewsPagedAsync(id, star, hasImage, page, pageSize);
            sanPham.DanhGias = reviews;

            return sanPham;
        }

        public async Task AddReviewAsync(int sanPhamId, int diem, string noiDung, string userName,
            List<IFormFile> images, string uploadPath)
        {
            var danhGia = new DanhGia
            {
                SanPhamId = sanPhamId,
                Diem = diem,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                TenNguoiDung = userName
            };

            await _repo.AddReviewAsync(danhGia, images, uploadPath);
        }

        public async Task<SanPham?> GetBasicAsync(int id)
        {
            return await _repo.GetBasicAsync(id);
        }

        public async Task<(List<string> kichThucs, List<string> mauSacs, List<DanhMuc> danhMucs)> GetFilterDropdownsAsync()
        {
            var kichThucs = await _repo.GetDistinctKichThucsAsync();
            var mauSacs = await _repo.GetDistinctMauSacsAsync();
            var danhMucs = await _repo.GetAllDanhMucsAsync();
            return (kichThucs, mauSacs, danhMucs);
        }
    }
}
