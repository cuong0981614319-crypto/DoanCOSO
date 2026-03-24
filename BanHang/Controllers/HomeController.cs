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

        public async Task<IActionResult> Index(int? maDanhMuc)
        {
            var query = _context.SanPhams
                .Include(x => x.DanhMuc)
                .AsQueryable();

            if (maDanhMuc.HasValue)
            {
                query = query.Where(x => x.MaDanhMuc == maDanhMuc.Value);
            }

            var products = await query
                .OrderBy(x => x.MaSanPham)
                .ToListAsync();

            ViewBag.MaDanhMucDangChon = maDanhMuc;

            return View(products);
        }
    }
}