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

            // Lọc theo khu vực hiển thị
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

            // Lọc theo danh mục
            if (maDanhMuc.HasValue)
            {
                query = query.Where(x => x.MaDanhMuc == maDanhMuc.Value);
            }

            // Lọc theo màu sắc
            if (!string.IsNullOrWhiteSpace(mauSac))
            {
                var mauSacLower = mauSac.Trim().ToLower();
                query = query.Where(x => x.MauSac != null && x.MauSac.ToLower() == mauSacLower);
            }

            // Lọc theo mức giá
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

            // Tổng số sản phẩm sau khi lọc
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1)
            {
                page = 1;
            }

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var products = await query
                .OrderBy(x => x.MaSanPham)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Danh sách danh mục cho bộ lọc
            ViewBag.DanhMucs = await _context.DanhMucs.ToListAsync();

            // Danh sách màu sắc cho bộ lọc
            ViewBag.MauSacs = await _context.SanPhams
                .Where(x => !string.IsNullOrEmpty(x.MauSac))
                .Select(x => x.MauSac)
                .Distinct()
                .ToListAsync();

            // Giữ lại giá trị lọc
            ViewBag.KhuVucId = khuVucId;
            ViewBag.MaDanhMuc = maDanhMuc;
            ViewBag.MucGia = mucGia;
            ViewBag.MauSac = mauSac;

            // Phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

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
                .Take(4)
                .ToListAsync();

            ViewBag.SanPhamCungLoai = sanPhamCungLoai;
            ViewBag.Quantity = quantity;
           

            return View("~/Views/Product/Details.cshtml", sanPham);
        }
    }
}