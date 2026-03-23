using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // KHÁCH + ADMIN: Đều thấy danh sách
        public async Task<IActionResult> Index()
        {
            var products = await _context.SanPhams.ToListAsync();
            return View(products);
        }

    
        // KHÁCH: Nhấn nút mua
        [Authorize] // Phải đăng nhập mới mua được
        public IActionResult Buy(int id)
        {
            TempData["Message"] = "Đã thêm vào giỏ hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}