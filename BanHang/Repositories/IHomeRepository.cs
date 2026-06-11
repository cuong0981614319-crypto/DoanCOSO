using BanHang.Models;

namespace BanHang.Repositories
{
    // ==============================
    // TẦNG REPOSITORY - Chỉ CRUD, không nghiệp vụ
    // ==============================

    public interface IHomeRepository
    {
        Task<List<KhuVucHienThi>> GetKhuVucsWithSanPhamsAsync();
        Task<DanhMuc?> GetDanhMucByIdAsync(int maDanhMuc);
        Task<List<SanPham>> GetBestSellersAsync(int minDaBan, int take);
        Task<List<RatingResult>> GetAllRatingsAsync();
        Task<List<SanPham>> SearchSanPhamsAsync(string keyword, int take);
    }

    public class RatingResult
    {
        public int SanPhamId { get; set; }
        public double Avg { get; set; }
        public int Count { get; set; }
    }
}
