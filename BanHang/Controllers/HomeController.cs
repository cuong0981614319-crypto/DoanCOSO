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
            var khuVucs = await _context.KhuVucHienThis
                .Include(k => k.SanPhams)
                    .ThenInclude(sp => sp.DanhMuc)
                .OrderBy(k => k.ThuTu)
                .ToListAsync();

            if (maDanhMuc.HasValue)
            {
                foreach (var kv in khuVucs)
                {
                    kv.SanPhams = kv.SanPhams?
                        .Where(sp => sp.MaDanhMuc == maDanhMuc.Value)
                        .ToList();
                }

                khuVucs = khuVucs
                    .Where(kv => kv.SanPhams != null && kv.SanPhams.Any())
                    .ToList();

                var danhMuc = await _context.DanhMucs
                    .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maDanhMuc.Value);

                ViewBag.TenDanhMucDangLoc = danhMuc?.TenDanhMuc;
            }

            ViewBag.MaDanhMucDangChon = maDanhMuc;

            return View("~/Views/Home/Index.cshtml", khuVucs);
        }

    }
}