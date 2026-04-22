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

            // Chỉ tính những đơn đã thanh toán vào doanh thu
            var donThanhToanQuery = _context.DonHangs
                .Where(x => x.DaThanhToan);

            // Khởi tạo Model
            var model = new ThongKeDoanhThuViewModel
            {
                // 1. Thống kê tổng quan (Các thẻ Card)
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

            // 2. Xử lý logic lọc cho bảng "Doanh thu theo ngày"
            var chartQuery = donThanhToanQuery.AsQueryable();

            if (fromDate.HasValue)
            {
                chartQuery = chartQuery.Where(x => x.NgayDat >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // .AddDays(1) để bao gồm cả dữ liệu của ngày kết thúc (tránh mất dữ liệu lúc 00:00)
                chartQuery = chartQuery.Where(x => x.NgayDat < toDate.Value.AddDays(1));
            }

            model.DoanhThuTheoNgay = await chartQuery
                .GroupBy(x => x.NgayDat.Date)
                .Select(g => new DoanhThuTheoNgayItem
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderByDescending(x => x.Ngay) // Để ngày mới nhất lên đầu bảng
                .ToListAsync();

            // Truyền lại ngày đã chọn để hiển thị trên Input ở View
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(model);
        }
    }
}