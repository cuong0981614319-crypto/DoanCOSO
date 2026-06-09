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
            var khuVucs = await _context.KhuVucHienThis
                .Include(k => k.SanPhams)
                    .ThenInclude(sp => sp.DanhMuc)
                .OrderBy(k => k.ThuTu)
                .ToListAsync();

            ViewBag.PromoProducts = await _productRepository.GetProductsNeverSoldAsync();

            // Lọc danh mục
            if (maDanhMuc.HasValue)
            {
                foreach (var kv in khuVucs)
                {
                    kv.SanPhams = kv.SanPhams
                        .Where(sp => sp.MaDanhMuc == maDanhMuc.Value)
                        .ToList();
                }

                khuVucs = khuVucs
                    .Where(kv => kv.SanPhams.Any())
                    .ToList();

                var danhMuc = await _context.DanhMucs
                    .FirstOrDefaultAsync(x => x.MaDanhMuc == maDanhMuc.Value);

                ViewBag.TenDanhMucDangLoc = danhMuc?.TenDanhMuc;
            }

            // Lọc từ khóa
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                foreach (var kv in khuVucs)
                {
                    kv.SanPhams = kv.SanPhams
                        .Where(sp => sp.TenSanPham != null &&
                                     sp.TenSanPham.ToLower().Contains(keyword))
                        .ToList();
                }

                khuVucs = khuVucs
                    .Where(kv => kv.SanPhams.Any())
                    .ToList();
            }

            ViewBag.MaDanhMucDangChon = maDanhMuc;
            ViewBag.Keyword = keyword;

            // Lấy sản phẩm bán chạy trực tiếp từ DB (DaBan > 5)
            ViewBag.BestSellers = await _context.SanPhams
                .Where(sp => sp.DaBan > 5)
                .OrderByDescending(sp => sp.DaBan)
                .Take(10)
                .ToListAsync();

            var allProducts = khuVucs.SelectMany(kv => kv.SanPhams);

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

            // Gán rating cho sản phẩm bán chạy
            var bestSellersList = ViewBag.BestSellers as List<SanPham>;
            if (bestSellersList != null)
            {
                foreach (var sp in bestSellersList)
                {
                    var r = ratings.FirstOrDefault(x => x.SanPhamId == sp.MaSanPham);
                    sp.AvgRating = r?.Avg ?? 0;
                    sp.TotalReviews = r?.Count ?? 0;
                }
            }

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