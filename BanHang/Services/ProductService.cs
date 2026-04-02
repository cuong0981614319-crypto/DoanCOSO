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
            var m = mauSac.ToLower();
            query = query.Where(x => x.MauSac.ToLower() == m);
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
            .OrderBy(x => x.MaSanPham)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalItems);
    }

    public async Task<SanPham?> GetDetails(int id)
    {
        return await _repo.GetById(id);
    }
}