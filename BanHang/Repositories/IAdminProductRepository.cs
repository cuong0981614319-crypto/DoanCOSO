using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IAdminProductRepository
    {
        Task<List<SanPham>> GetAllWithRelationsAsync();
        Task<SanPham?> GetByIdWithImagesAsync(int id);
        Task<SanPham?> GetByIdAsync(int id);
        Task AddAsync(SanPham sanPham);
        Task UpdateAsync(SanPham sanPham);
        Task RemoveImageAsync(HinhAnhSanPham image);
        Task AddImageAsync(HinhAnhSanPham image);
        Task DeleteAsync(SanPham sanPham);
        Task<List<DanhMuc>> GetDanhMucsAsync();
        Task<List<KhuVucHienThi>> GetKhuVucHienThisAsync();
    }
}
