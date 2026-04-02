using BanHang.Models;

public interface IProductRepository
{
    IQueryable<SanPham> GetQuery();
    Task<SanPham?> GetById(int id);
    Task<List<string>> GetMauSacs();
}