using BanHang.Migrations;
using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> Details(int id)
    {
        var sanPham = await _service.GetDetails(id);

        // 🔍 DEBUG
        var count = await _context.DanhGias.CountAsync(d => d.SanPhamId == id);
        ViewBag.DebugCount = $"Đánh giá: {count} (SanPhamId={id})";

        sanPham.DanhGias = await _context.DanhGias
            .Where(d => d.SanPhamId == id)
            .OrderByDescending(d => d.NgayTao)
            .ToListAsync();

        return View(sanPham);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int sanPhamId, int diem, string noiDung)
    {
        var danhGia = new DanhGia
        {
            SanPhamId = sanPhamId,
            Diem = diem,
            NoiDung = noiDung,
            NgayTao = DateTime.Now,
            TenNguoiDung = User.Identity.Name
        };

        _context.DanhGias.Add(danhGia);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = sanPhamId });
    }
    public IActionResult Review(int id)
    {
        var product = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == id);
        if (product == null) return NotFound();

        ViewBag.ProductId = id;
        ViewBag.ProductName = product.TenSanPham;
        ViewBag.ProductPrice = product.Gia;
        ViewBag.ProductImage = product.HinhAnh;
        ViewBag.Reviews = _context.DanhGias.Where(d => d.SanPhamId == id).ToList();

        // ✅ Nếu view ở Views/Product/Danhgia.cshtml
        return View("Danhgia");

        // Hoặc nếu view ở Views/DonHang/
        // return View("~/Views/DonHang/Danhgia.cshtml");
    }
    public IActionResult Danhgia(int id)
    {
        var product = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == id);
        if (product == null) return NotFound();

        ViewBag.ProductId = id;
        ViewBag.ProductName = product.TenSanPham;
        ViewBag.ProductPrice = product.Gia;
        ViewBag.ProductImage = product.HinhAnh;
        ViewBag.Reviews = _context.DanhGias.Where(d => d.SanPhamId == id).ToList();

        return View(); // Tìm Views/Product/DanhGia.cshtml
    }
}