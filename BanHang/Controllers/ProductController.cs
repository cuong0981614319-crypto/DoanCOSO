using BanHang.Models;
using BanHang.Services;
using Microsoft.AspNetCore.Mvc;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductDetailService _productDetailService;

    public ProductController(IProductService productService, IProductDetailService productDetailService)
    {
        _productService = productService;
        _productDetailService = productDetailService;
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

        var (kichThucs, mauSacs, danhMucs) = await _productDetailService.GetFilterDropdownsAsync();

        ViewBag.DanhMucs = danhMucs
            .GroupBy(x => x.TenDanhMuc.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        ViewBag.CurrentMaDanhMuc = maDanhMuc;
        ViewBag.KichThucs = kichThucs;
        ViewBag.CurrentKichThuc = kichthuoc;
        ViewBag.MauSacs = mauSacs;

        var (products, totalItems) = await _productService.GetFilteredProducts(
            khuVucId, maDanhMuc, mucGia, mauSac, page, pageSize);

        ViewBag.TotalItems = totalItems;

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, int page = 1, int? star = null, bool? hasImage = null)
    {
        int pageSize = 10;

        var sanPham = await _productDetailService.GetDetailsAsync(id, page, star, hasImage, pageSize);
        if (sanPham == null) return NotFound();

        var (_, totalReviews) = await GetReviewCounts(id, star, hasImage, page, pageSize);

        ViewBag.SanPhamCungLoai = await GetSameCategoryProducts(id, sanPham.MaDanhMuc);
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);
        ViewBag.CurrentStar = star;
        ViewBag.HasImage = hasImage;

        return View(sanPham);
    }

    // Helper cục bộ - gọi lại service để lấy total count cho phân trang
    private async Task<(List<object> items, int total)> GetReviewCounts(int id, int? star, bool? hasImage, int page, int pageSize)
    {
        // Lấy tổng số review cho phân trang - gọi qua IProductDetailService
        var sanPham = await _productDetailService.GetDetailsAsync(id, page, star, hasImage, pageSize);
        return (new List<object>(), sanPham?.DanhGias?.Count ?? 0);
    }

    private async Task<List<BanHang.Models.SanPham>> GetSameCategoryProducts(int sanPhamId, int? maDanhMuc)
    {
        if (maDanhMuc == null) return new();
        var (products, _) = await _productService.GetFilteredProducts(null, maDanhMuc, null, null, 1, 10);
        return products.Where(p => p.MaSanPham != sanPhamId).Take(10).ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int sanPhamId, int diem, string noiDung, List<IFormFile> images)
    {
        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/reviews");
        await _productDetailService.AddReviewAsync(sanPhamId, diem, noiDung, User.Identity!.Name!, images, uploadPath);
        return RedirectToAction("Details", new { id = sanPhamId });
    }

    [HttpGet]
    public async Task<IActionResult> Danhgia(int id)
    {
        var product = await _productDetailService.GetBasicAsync(id);
        if (product == null) return NotFound();

        ViewBag.ProductId = id;
        ViewBag.ProductName = product.TenSanPham;
        ViewBag.ProductImage = product.HinhAnh;
        ViewBag.GiaGoc = product.Gia;
        ViewBag.ProductPrice = product.GiaKhuyenMai;
        ViewBag.PhanTramGiam = product.PhanTramGiam;

        return View();
    }
}