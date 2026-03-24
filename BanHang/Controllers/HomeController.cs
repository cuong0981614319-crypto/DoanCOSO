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
            // Lấy khu vực + sản phẩm
            var khuVucs = await _context.KhuVucHienThis
                .Include(k => k.SanPhams)
                    .ThenInclude(sp => sp.DanhMuc)
                .ToListAsync();

            // Nếu có lọc danh mục
            if (maDanhMuc.HasValue)
            {
                foreach (var kv in khuVucs)
                {
                    kv.SanPhams = kv.SanPhams?
                        .Where(sp => sp.MaDanhMuc == maDanhMuc.Value)
                        .ToList();
                }
            }

            ViewBag.MaDanhMucDangChon = maDanhMuc;

            return View(khuVucs);
        }
    }
}