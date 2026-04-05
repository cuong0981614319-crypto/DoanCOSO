using Microsoft.AspNetCore.Mvc;

public class ProductController : Controller
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(
        int? khuVucId,
        int? maDanhMuc,
        string? mucGia,
        string? mauSac,
        int page = 1)
    {
        int pageSize = 16;

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