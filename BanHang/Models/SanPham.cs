using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace BanHang.Models
{
    public class SanPham
    {
        [Key]
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public string? MoTa { get; set; }
        public string? MauSac { get; set; }

        public int MaDanhMuc { get; set; }
        public DanhMuc? DanhMuc { get; set; }

        public int? KhuVucHienThiId { get; set; }
        public KhuVucHienThi? KhuVucHienThi { get; set; }

        // ảnh đại diện
        public string? HinhAnh { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // upload nhiều ảnh
        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        public List<HinhAnhSanPham>? HinhAnhSanPhams { get; set; }
        public int DaBan { get; internal set; }
    }
}