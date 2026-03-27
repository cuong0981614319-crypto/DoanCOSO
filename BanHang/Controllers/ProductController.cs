using BanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? khuVucId,
            int? maDanhMuc,
            string? mucGia,
            string? mauSac,
            int page = 1)
        {
            const int pageSize = 16;

            IQueryable<SanPham> query = _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.KhuVucHienThi);

            if (khuVucId.HasValue)
            {
                query = query.Where(x => x.KhuVucHienThiId == khuVucId.Value);

                var khuVuc = await _context.KhuVucHienThis
                    .FirstOrDefaultAsync(x => x.Id == khuVucId.Value);

                ViewBag.TenKhuVuc = khuVuc != null ? khuVuc.Ten : "Sản phẩm";
            }
            else
            {
                ViewBag.TenKhuVuc = "Tất cả sản phẩm";
            }

            if (maDanhMuc.HasValue)
            {
                query = query.Where(x => x.MaDanhMuc == maDanhMuc.Value);
            }

            if (!string.IsNullOrWhiteSpace(mauSac))
            {
                var mauSacLower = mauSac.Trim().ToLower();
                query = query.Where(x => x.MauSac != null && x.MauSac.ToLower() == mauSacLower);
            }

            if (!string.IsNullOrWhiteSpace(mucGia))
            {
                switch (mucGia)
                {
                    case "duoi1tr":
                        query = query.Where(x => x.Gia < 1000000);
                        break;

                    case "1tr-5tr":
                        query = query.Where(x => x.Gia >= 1000000 && x.Gia <= 5000000);
                        break;

                    case "5tr-10tr":
                        query = query.Where(x => x.Gia > 5000000 && x.Gia <= 10000000);
                        break;

                    case "tren10tr":
                        query = query.Where(x => x.Gia > 10000000);
                        break;
                }
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var products = await query
                .OrderBy(x => x.MaSanPham)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.DanhMucs = await _context.DanhMucs.ToListAsync();

            ViewBag.MauSacs = await _context.SanPhams
                .Where(x => !string.IsNullOrEmpty(x.MauSac))
                .Select(x => x.MauSac)
                .Distinct()
                .ToListAsync();

            ViewBag.KhuVucId = khuVucId;
            ViewBag.MaDanhMuc = maDanhMuc;
            ViewBag.MucGia = mucGia;
            ViewBag.MauSac = mauSac;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int quantity = 1)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var sanPham = await _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.HinhAnhSanPhams)
                .FirstOrDefaultAsync(x => x.MaSanPham == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            var sanPhamCungLoai = await _context.SanPhams
                .Where(x => x.MaDanhMuc == sanPham.MaDanhMuc && x.MaSanPham != sanPham.MaSanPham)
                .Take(8)
                .ToListAsync();

            ViewBag.SanPhamCungLoai = sanPhamCungLoai;
            ViewBag.Quantity = quantity;

            return View("~/Views/Product/Details.cshtml", sanPham);
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            var products = new List<SanPham>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                products = _context.SanPhams
                    .Where(x => x.TenSanPham != null && x.TenSanPham.Contains(keyword))
                    .OrderBy(x => x.TenSanPham)
                    .ToList();
            }

            ViewBag.Keyword = keyword;
            return View(products);
        }

    }
}