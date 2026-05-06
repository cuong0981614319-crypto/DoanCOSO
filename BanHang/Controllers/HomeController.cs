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
            // 4. Thiết lập ViewBag cho giao diện
            if (maDanhMuc.HasValue)
            {
                var danhMuc = await _context.DanhMucs
                    .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maDanhMuc.Value);
                ViewBag.TenDanhMucDangLoc = danhMuc?.TenDanhMuc;
            }

            ViewBag.MaDanhMucDangChon = maDanhMuc;
            ViewBag.Keyword = keyword;

            // ================== THÊM Ở ĐÂY ==================
            var allProducts = khuVucs.SelectMany(kv => kv.SanPhams ?? new List<SanPham>());

            // 👉 lấy 1 lần (tối ưu)
            var ratings = await _context.DanhGias
                .GroupBy(d => d.SanPhamId)
                .Select(g => new
                {
                    SanPhamId = g.Key,
                    Avg = g.Average(x => x.Diem),
                    Count = g.Count()
                })
                .ToListAsync();

            foreach (var sp in allProducts)
            {
                var r = ratings.FirstOrDefault(x => x.SanPhamId == sp.MaSanPham);
                sp.AvgRating = r?.Avg ?? 0;
                sp.TotalReviews = r?.Count ?? 0;
            }
            // ================== END ==================

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