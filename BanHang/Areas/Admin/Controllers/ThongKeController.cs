using BanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // Chỉ tính những đơn đã thanh toán
            var donThanhToanQuery = _context.DonHangs.Where(x => x.DaThanhToan);

            var model = new ThongKeDoanhThuViewModel
            {
                DoanhThuHomNay = await donThanhToanQuery
                    .Where(x => x.NgayDat.Date == today)
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                DoanhThuThangNay = await donThanhToanQuery
                    .Where(x => x.NgayDat >= firstDayOfMonth)
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                TongDoanhThu = await donThanhToanQuery
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                SoDonDaThanhToan = await donThanhToanQuery.CountAsync()
            };

            // --- Bổ sung Thống kê Sản phẩm theo Danh mục (cho Biểu đồ tròn) ---
            var thongKeDanhMuc = await _context.DanhMucs
                .Select(dm => new {
                    Label = dm.TenDanhMuc,
                    Count = dm.SanPhams.Count()
                }).ToListAsync();

            ViewBag.Labels = thongKeDanhMuc.Select(x => x.Label).ToList();
            ViewBag.Counts = thongKeDanhMuc.Select(x => x.Count).ToList();
            // -----------------------------------------------------------

            // Logic lọc Doanh thu theo ngày... (giữ nguyên như code của bạn)
            var chartQuery = donThanhToanQuery.AsQueryable();
            if (fromDate.HasValue) chartQuery = chartQuery.Where(x => x.NgayDat >= fromDate.Value);
            if (toDate.HasValue) chartQuery = chartQuery.Where(x => x.NgayDat < toDate.Value.AddDays(1));

            model.DoanhThuTheoNgay = await chartQuery
                .GroupBy(x => x.NgayDat.Date)
                .Select(g => new DoanhThuTheoNgayItem
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderByDescending(x => x.Ngay)
                .ToListAsync();

            return View(model);
        }
    }
}