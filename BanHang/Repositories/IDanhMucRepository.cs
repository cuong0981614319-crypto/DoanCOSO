using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IDanhMucRepository
    {
        Task<List<DanhMuc>> GetAllAsync();
        Task<DanhMuc?> GetByIdAsync(int id);
        Task AddAsync(DanhMuc danhMuc);
        Task UpdateAsync(DanhMuc danhMuc);
        Task<bool> DeleteAsync(int id);
    }
}
