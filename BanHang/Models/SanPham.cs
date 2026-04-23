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

        public int? MaDanhMuc { get; set; }

        [ForeignKey(nameof(MaDanhMuc))]
        public DanhMuc? DanhMuc { get; set; }

        public int? KhuVucHienThiId { get; set; }

        [ForeignKey(nameof(KhuVucHienThiId))]
        public KhuVucHienThi? KhuVucHienThi { get; set; }

        public string? HinhAnh { get; set; }
        public string? kichthuc { get; set; }
        public string ? chatlieu { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        [NotMapped]
        public List<int>? DeletedImageIds { get; set; }

        [NotMapped]
        public string? SelectedThumbnail { get; set; }

        public List<HinhAnhSanPham>? HinhAnhSanPhams { get; set; } = new();

        public int DaBan { get; internal set; }
        public decimal GiaKhuyenMai
        {
            get
            {
                if (DaBan < 10 || DaBan >0)
                {
                    return Gia * 0.8m;
                }
                else if (DaBan < 20 || DaBan > 10)
                {
                    return Gia * 0.92m;
                }
                else if (DaBan == 0)
                {
                    return Gia * 0.7m;
                }
                    return Gia;
            }
        }
        public int PhanTramGiam
        {
            get
            {
                if (DaBan == 0) return 30;
                if (DaBan < 10 || DaBan >0) return 20;
                if (DaBan < 20) return 8;
                return 0; 
            }
        }
    }
}