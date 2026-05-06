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
    public async Task<IActionResult> Details(
       int id,
       int page = 1,
       int? star = null,
       bool? hasImage = null)
    {
        int pageSize = 10;

        var sanPham = await _service.GetDetails(id);

        var query = _context.DanhGias
            .Where(d => d.SanPhamId == id)
            .Include(d => d.Images)
            .AsQueryable();

        // lọc sao
        if (star.HasValue && star > 0)
        {
            query = query.Where(d => d.Diem == star);
        }

        // lọc có ảnh
        if (hasImage == true)
        {
            query = query.Where(d => d.Images.Any());
        }

        int totalItems = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(d => d.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        sanPham.DanhGias = reviews;

        // ViewBag cho phân trang
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        ViewBag.CurrentStar = star;
        ViewBag.HasImage = hasImage;

        return View(sanPham);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int sanPhamId, int diem, string noiDung, List<IFormFile> images)
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

        // 📁 đường dẫn lưu ảnh
        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/reviews");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // 📸 lưu nhiều ảnh
        if (images != null && images.Count > 0)
        {
            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var img = new DanhGiaImage
                    {
                        DanhGiaId = danhGia.Id,
                        ImageUrl = "/images/reviews/" + fileName
                    };

                    _context.DanhGiaImages.Add(img);
                }
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { id = sanPhamId });
    }
    [HttpGet]
    public IActionResult Danhgia(int id)
    {
        var product = _context.SanPhams.FirstOrDefault(p => p.MaSanPham == id);
        if (product == null) return NotFound();

        ViewBag.ProductId = id;
        ViewBag.ProductName = product.TenSanPham;
        ViewBag.ProductPrice = product.Gia;
        ViewBag.ProductImage = product.HinhAnh;

        return View(); // phải có Views/Product/Danhgia.cshtml
    }
}