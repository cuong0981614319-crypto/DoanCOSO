using BanHang.Models;
using Microsoft.EntityFrameworkCore;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<SanPham> GetQuery()
    {
        return _context.SanPhams
            .Include(x => x.DanhMuc)
            .Include(x => x.KhuVucHienThi);
    }

    public async Task<SanPham?> GetById(int id)
    {
        return await _context.SanPhams
            .Include(x => x.DanhMuc)
            .Include(x => x.HinhAnhSanPhams)
            .FirstOrDefaultAsync(x => x.MaSanPham == id);
    }

    public async Task<List<string>> GetMauSacs()
    {
        return await _context.SanPhams
            .Where(x => !string.IsNullOrEmpty(x.MauSac))
            .Select(x => x.MauSac)
            .Distinct()
            .ToListAsync();
    }
}