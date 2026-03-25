using BanHang.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<SanPham> SanPhams { get; set; }
    public DbSet<DanhMuc> DanhMucs { get; set; } // Sửa từ KhachHangs thành DanhMucs
    public DbSet<DonHang> DonHangs { get; set; }
    public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<KhuVucHienThi> KhuVucHienThis { get; set; }
    public DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 👇 đặt đoạn của bạn ở đây
        modelBuilder.Entity<HinhAnhSanPham>()
            .HasOne(x => x.SanPham)
            .WithMany(x => x.HinhAnhSanPhams)
            .HasForeignKey(x => x.MaSanPham)
            .OnDelete(DeleteBehavior.Cascade);
    }

}