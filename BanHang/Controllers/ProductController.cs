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

        public async Task<IActionResult> Index()
        {
            var products = await _context.SanPhams
                .Include(x => x.DanhMuc)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var sanPham = await _context.SanPhams
                .Include(x => x.DanhMuc)
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

            return View("~/Views/Product/Details.cshtml", sanPham);
        }
    }
}