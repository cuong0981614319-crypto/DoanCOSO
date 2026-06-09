using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanHang.Models;

namespace BanHang.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public MenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy danh sách danh mục từ database
            var items = await _context.DanhMucs.ToListAsync();
            
            // Lọc trùng lặp theo tên danh mục (xử lý chữ hoa/thường và khoảng trắng)
            var uniqueItems = items
                .GroupBy(x => x.TenDanhMuc.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            return View(uniqueItems);
        }
    }
}