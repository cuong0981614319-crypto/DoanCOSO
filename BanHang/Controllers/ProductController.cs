using BanHang.Migrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class ProductController : Controller
{
    private readonly IProductService _service;
    private readonly ApplicationDbContext _context;

    public ProductController(IProductService service, ApplicationDbContext context)
    {
        _service = service; 
        _context = context;
    }

    public async Task<IActionResult> Index(
        int? khuVucId,
        int? maDanhMuc,
        string? mucGia,
        string? mauSac,
        int? kichthuoc,
        int page = 1)
    {
        int pageSize = 16;
        ViewBag.DanhMucs = await _context.DanhMucs.ToListAsync();

        // Lưu lại ID đang chọn để giữ trạng thái "selected"
        ViewBag.CurrentMaDanhMuc = maDanhMuc;
        ViewBag.KichThucs = await _context.SanPhams
        .Select(p => p.kichthuc)
        .Distinct()
        .Where(k => !string.IsNullOrEmpty(k))
        .ToListAsync();

        // Lưu lại giá trị đang chọn để giữ trạng thái trên giao diện
        ViewBag.CurrentKichThuc = kichthuoc;
        ViewBag.MauSacs = await _context.SanPhams
            .Select(p => p.MauSac)
            .Distinct()
            .Where(m => !string.IsNullOrEmpty(m))
            .ToListAsync();
        var (products, totalItems) = await _service.GetFilteredProducts(
            khuVucId, maDanhMuc, mucGia, mauSac, page, pageSize);

        ViewBag.TotalItems = totalItems;

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, int quantity = 1)
    {
        // 🔥 sửa lại đúng tên hàm
        var sanPham = await _service.GetDetails(id);

        if (sanPham == null)
        {
            return NotFound();
        }

        // 🔥 sản phẩm cùng loại
        var sanPhamCungLoai = await _service.GetRelatedProducts(id, sanPham.MaDanhMuc);

        ViewBag.SanPhamCungLoai = sanPhamCungLoai;
        ViewBag.Quantity = quantity;

        return View(sanPham);
    }

}