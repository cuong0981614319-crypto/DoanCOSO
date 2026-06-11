using BanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHang.Repositories
{
    public class ThongKeRepository : IThongKeRepository
    {
        private readonly ApplicationDbContext _context;

        public ThongKeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ThongKeDoanhThuViewModel> GetThongKeAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var query = _context.DonHangs.Where(x => x.DaThanhToan);

            return new ThongKeDoanhThuViewModel
            {
                DoanhThuHomNay = await query
                    .Where(x => x.NgayDat.Date == today)
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                DoanhThuThangNay = await query
                    .Where(x => x.NgayDat >= firstDayOfMonth)
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                TongDoanhThu = await query
                    .SumAsync(x => (decimal?)x.TongTien) ?? 0,

                SoDonDaThanhToan = await query.CountAsync()
            };
        }

        public async Task<List<DoanhThuTheoNgayItem>> GetDoanhThuTheoNgayAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.DonHangs.Where(x => x.DaThanhToan).AsQueryable();

            if (fromDate.HasValue) query = query.Where(x => x.NgayDat >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.NgayDat < toDate.Value.AddDays(1));

            return await query
                .GroupBy(x => x.NgayDat.Date)
                .Select(g => new DoanhThuTheoNgayItem
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderByDescending(x => x.Ngay)
                .ToListAsync();
        }

        public async Task<List<(string Label, int Count)>> GetThongKeDanhMucAsync()
        {
            var data = await _context.DanhMucs
                .Select(dm => new { dm.TenDanhMuc, Count = dm.SanPhams.Count() })
                .ToListAsync();

            return data.Select(x => (x.TenDanhMuc, x.Count)).ToList();
        }
    }
}
