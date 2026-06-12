using BanHang.Models;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<(List<SanPham>, int)> GetFilteredProducts(
        int? khuVucId,
        int? maDanhMuc,
        string? mucGia,
        string? mauSac,
        string? keyword,
        int page,
        int pageSize)
    {
        var query = _repo.GetQuery();

        if (khuVucId.HasValue)
            query = query.Where(x => x.KhuVucHienThiId == khuVucId);

        if (maDanhMuc.HasValue)
            query = query.Where(x => x.MaDanhMuc == maDanhMuc);

        if (!string.IsNullOrWhiteSpace(mauSac))
        {
            var m = mauSac.Trim().ToLower();
            query = query.Where(x =>
                x.MauSac != null &&
                x.MauSac.ToLower() == m);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim().ToLower();

            query = query.Where(x =>
                x.TenSanPham.ToLower().Contains(key) ||
                (x.MoTa != null && x.MoTa.ToLower().Contains(key)) ||
                (x.MauSac != null && x.MauSac.ToLower().Contains(key)) ||
                (x.chatlieu != null && x.chatlieu.ToLower().Contains(key)) ||
                (x.kichthuc != null && x.kichthuc.ToLower().Contains(key)) ||
                (x.DanhMuc != null && x.DanhMuc.TenDanhMuc.ToLower().Contains(key)));
        }

        if (!string.IsNullOrWhiteSpace(mucGia))
        {
            switch (mucGia)
            {
                case "duoi1tr":
                    query = query.Where(x => x.Gia < 1000000);
                    break;

                case "1tr-5tr":
                    query = query.Where(x => x.Gia >= 1000000 && x.Gia <= 5000000);
                    break;

                case "5tr-10tr":
                    query = query.Where(x => x.Gia > 5000000 && x.Gia <= 10000000);
                    break;

                case "tren10tr":
                    query = query.Where(x => x.Gia > 10000000);
                    break;
            }
        }

        var totalItems = await query.CountAsync();

        var data = await query
            .Include(x => x.DanhMuc)
            .OrderBy(x => x.MaSanPham)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalItems);
    }

    public async Task<SanPham?> GetDetails(int id)
    {
        return await _repo.GetQuery()
            .Include(p => p.DanhMuc)
            .Include(p => p.HinhAnhSanPhams)
            .FirstOrDefaultAsync(p => p.MaSanPham == id);
    }

    public async Task<List<SanPham>> GetRelatedProducts(int productId, int? maDanhMuc)
    {
        if (maDanhMuc == null)
            return new List<SanPham>();

        return await _repo.GetQuery()
            .Include(p => p.DanhMuc)
            .Where(p => p.MaDanhMuc == maDanhMuc && p.MaSanPham != productId)
            .OrderByDescending(p => p.DaBan)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<SanPham>> SearchProducts(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<SanPham>();

        var key = keyword.Trim().ToLower();

        return await _repo.GetQuery()
            .Include(x => x.DanhMuc)
            .Where(x =>
                x.TenSanPham.ToLower().Contains(key) ||
                (x.MoTa != null && x.MoTa.ToLower().Contains(key)) ||
                (x.MauSac != null && x.MauSac.ToLower().Contains(key)) ||
                (x.chatlieu != null && x.chatlieu.ToLower().Contains(key)) ||
                (x.kichthuc != null && x.kichthuc.ToLower().Contains(key)) ||
                (x.DanhMuc != null && x.DanhMuc.TenDanhMuc.ToLower().Contains(key)))
            .ToListAsync();
    }
}