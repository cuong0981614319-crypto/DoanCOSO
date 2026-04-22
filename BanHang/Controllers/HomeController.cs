using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;

        // C?p nh?t Constructor ?? nh?n c? context và repository
        public HomeController(ApplicationDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        // CH? GI? L?I M?T HÀM INDEX NÀY
        public async Task<IActionResult> Index(int? maDanhMuc, string? keyword)
        {
            // 1. L?y d? li?u khu v?c hi?n th? và s?n ph?m
            var khuVucs = await _context.KhuVucHienThis
                .Include(k => k.SanPhams)
                    .ThenInclude(sp => sp.DanhMuc)
                .OrderBy(k => k.ThuTu)
                .ToListAsync();

            // 2. L?Y S?N PH?M KHUY?N MÃI (Bán = 0) qua Repository
            var promoProducts = await _productRepository.GetProductsNeverSoldAsync();
            ViewBag.PromoProducts = promoProducts;

            // 3. Logic l?c theo danh m?c ho?c t? khóa (N?u có)
            if (maDanhMuc.HasValue || !string.IsNullOrWhiteSpace(keyword))
            {
                var tuKhoa = keyword?.Trim().ToLower();

                foreach (var kv in khuVucs)
                {
                    if (kv.SanPhams == null) continue;

                    var sanPhams = kv.SanPhams.AsEnumerable();

                    if (maDanhMuc.HasValue)
                    {
                        sanPhams = sanPhams.Where(sp => sp.MaDanhMuc == maDanhMuc.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(tuKhoa))
                    {
                        sanPhams = sanPhams.Where(sp =>
                            !string.IsNullOrEmpty(sp.TenSanPham) &&
                            sp.TenSanPham.ToLower().Contains(tuKhoa));
                    }

                    kv.SanPhams = sanPhams.ToList();
                }

                khuVucs = khuVucs
                    .Where(kv => kv.SanPhams != null && kv.SanPhams.Any())
                    .ToList();
            }

            // 4. Thi?t l?p ViewBag cho giao di?n
            if (maDanhMuc.HasValue)
            {
                var danhMuc = await _context.DanhMucs
                    .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maDanhMuc.Value);
                ViewBag.TenDanhMucDangLoc = danhMuc?.TenDanhMuc;
            }

            ViewBag.MaDanhMucDangChon = maDanhMuc;
            ViewBag.Keyword = keyword;

            return View(khuVucs);
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new List<object>());
            }

            keyword = keyword.Trim();

            var products = _context.SanPhams
                .Where(x => x.TenSanPham != null && x.TenSanPham.Contains(keyword))
                .OrderBy(x => x.TenSanPham)
                .Take(8)
                .Select(x => new
                {
                    id = x.MaSanPham,
                    name = x.TenSanPham,
                    price = x.Gia,
                    image = x.HinhAnh,
                    url = Url.Action("Details", "Product", new { id = x.MaSanPham })
                })
                .ToList();

            return Json(products);
        }
    }
}