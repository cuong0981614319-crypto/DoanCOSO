using System.Security.Claims;
using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHangs = await _context.DonHangs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var donHang = await _context.DonHangs
                .Include(x => x.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(x => x.MaDonHang == id && x.UserId == userId);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }
    }
}