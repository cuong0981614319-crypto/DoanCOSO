using BanHang.Models;

namespace BanHang.Repositories
{
    public interface IKhuVucRepository
    {
        Task<List<KhuVucHienThi>> GetAllAsync();
        Task<KhuVucHienThi?> GetByIdWithSanPhamsAsync(int id);
        Task AddAsync(KhuVucHienThi kv);
        Task UpdateAsync(KhuVucHienThi kv);
        Task<bool> DeleteAsync(int id);
    }
}
