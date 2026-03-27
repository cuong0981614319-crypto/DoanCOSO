using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? maDanhMuc, string? keyword)
        {
            var khuVucs = await _context.KhuVucHienThis
                .Include(k => k.SanPhams)
                    .ThenInclude(sp => sp.DanhMuc)
                .OrderBy(k => k.ThuTu)
                .ToListAsync();

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

            if (maDanhMuc.HasValue)
            {
                var danhMuc = await _context.DanhMucs
                    .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maDanhMuc.Value);

                ViewBag.TenDanhMucDangLoc = danhMuc?.TenDanhMuc;
            }

            ViewBag.MaDanhMucDangChon = maDanhMuc;
            ViewBag.Keyword = keyword;

            return View("~/Views/Home/Index.cshtml", khuVucs);
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