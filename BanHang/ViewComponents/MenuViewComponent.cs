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
            return View(items);
        }
    }
}