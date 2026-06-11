using BanHang.Models;

namespace BanHang.Services
{
    public interface IProductDetailService
    {
        Task<SanPham?> GetDetailsAsync(int id, int page, int? star, bool? hasImage, int pageSize);
        Task AddReviewAsync(int sanPhamId, int diem, string noiDung, string userName, List<IFormFile> images, string uploadPath);
        Task<SanPham?> GetBasicAsync(int id);
        Task<(List<string> kichThucs, List<string> mauSacs, List<DanhMuc> danhMucs)> GetFilterDropdownsAsync();
    }
}
