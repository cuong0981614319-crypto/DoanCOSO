using BanHang.Models;
using BanHang.Services;
using Microsoft.AspNetCore.Mvc;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductDetailService _productDetailService;

    public ProductController(
        IProductService productService,
        IProductDetailService productDetailService)
    {
        _productService = productService;
        _productDetailService = productDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? khuVucId,
        int? maDanhMuc,
        string? mucGia,
        string? mauSac,
        int? kichthuoc,
        string? keyword,
        int page = 1)
    {
        mucGia = string.IsNullOrWhiteSpace(mucGia) ? null : mucGia;
        mauSac = string.IsNullOrWhiteSpace(mauSac) ? null : mauSac;
        keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

        int pageSize = 15;

        var (kichThucs, mauSacs, danhMucs) =
            await _productDetailService.GetFilterDropdownsAsync();

        ViewBag.DanhMucs = danhMucs
            .GroupBy(x => x.TenDanhMuc.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        ViewBag.CurrentMaDanhMuc = maDanhMuc;
        ViewBag.KichThucs = kichThucs;
        ViewBag.CurrentKichThuc = kichthuoc;
        ViewBag.MauSacs = mauSacs;
        ViewBag.MaDanhMuc = maDanhMuc;
        ViewBag.MucGia = mucGia;
        ViewBag.MauSac = mauSac;
        ViewBag.KhuVucId = khuVucId;
        ViewBag.Keyword = keyword;

        var (products, totalItems) = await _productService.GetFilteredProducts(
            khuVucId,
            maDanhMuc,
            mucGia,
            mauSac,
            keyword,
            page,
            pageSize);

        ViewBag.TotalItems = totalItems;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Suggest(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Json(new { items = new List<object>(), total = 0 });

        var products = await _productService.SearchProducts(keyword);

        var items = products.Take(5).Select(x => new
        {
            id = x.MaSanPham,
            name = x.TenSanPham,
            price = x.GiaKhuyenMai,
            oldPrice = x.Gia,
            image = string.IsNullOrWhiteSpace(x.HinhAnh) ? "/img/no-image.png" : x.HinhAnh
        });

        return Json(new
        {
            items,
            total = products.Count
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        int page = 1,
        int? star = null,
        bool? hasImage = null)
    {
        int pageSize = 10;

        var sanPham = await _productDetailService.GetDetailsAsync(
            id,
            page,
            star,
            hasImage,
            pageSize);

        if (sanPham == null)
            return NotFound();

        var (_, totalReviews) = await GetReviewCounts(
            id,
            star,
            hasImage,
            page,
            pageSize);

        ViewBag.SanPhamCungLoai = await GetSameCategoryProducts(id, sanPham.MaDanhMuc);
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);
        ViewBag.CurrentStar = star;
        ViewBag.HasImage = hasImage;

        return View(sanPham);
    }

    private async Task<(List<object> items, int total)> GetReviewCounts(
        int id,
        int? star,
        bool? hasImage,
        int page,
        int pageSize)
    {
        var sanPham = await _productDetailService.GetDetailsAsync(
            id,
            page,
            star,
            hasImage,
            pageSize);

        return (new List<object>(), sanPham?.DanhGias?.Count ?? 0);
    }

    private async Task<List<SanPham>> GetSameCategoryProducts(
        int sanPhamId,
        int? maDanhMuc)
    {
        if (maDanhMuc == null)
            return new List<SanPham>();

        var (products, _) = await _productService.GetFilteredProducts(
            null,
            maDanhMuc,
            null,
            null,
            null,
            1,
            10);

        return products
            .Where(p => p.MaSanPham != sanPhamId)
            .Take(10)
            .ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(
        int sanPhamId,
        int diem,
        string noiDung,
        List<IFormFile> images)
    {
        string uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/images/reviews");

        await _productDetailService.AddReviewAsync(
            sanPhamId,
            diem,
            noiDung,
            User.Identity!.Name!,
            images,
            uploadPath);

        return RedirectToAction("Details", new { id = sanPhamId });
    }

    [HttpGet]
    public async Task<IActionResult> Danhgia(int id)
    {
        var product = await _productDetailService.GetBasicAsync(id);

        if (product == null)
            return NotFound();

        ViewBag.ProductId = id;
        ViewBag.ProductName = product.TenSanPham;
        ViewBag.ProductImage = product.HinhAnh;
        ViewBag.GiaGoc = product.Gia;
        ViewBag.ProductPrice = product.GiaKhuyenMai;
        ViewBag.PhanTramGiam = product.PhanTramGiam;

        return View();
    }
}