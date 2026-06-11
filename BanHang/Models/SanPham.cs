using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;

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

        [Display(Name = "Ngày thêm")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? NgayThem { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        [NotMapped]
        public List<int>? DeletedImageIds { get; set; }

        [NotMapped]
        public string? SelectedThumbnail { get; set; }

        public List<HinhAnhSanPham>? HinhAnhSanPhams { get; set; } = new();
        public List<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public int DaBan { get; internal set; }
        [NotMapped]
        public double AvgRating { get; set; }

        [NotMapped]
        public int TotalReviews { get; set; }
        public decimal GiaKhuyenMai
        {
            get
            {
                int phanTram = PhanTramGiam;
                return Gia - (Gia * phanTram / 100);
            }
        }

        public int PhanTramGiam
        {
            get
            {
                // Ngày 1/6/2026 giảm toàn bộ sản phẩm 10%
                if (DateTime.Today.Date == new DateTime(2026, 6, 1))
                {
                    return 10;
                }

                // Sản phẩm thêm năm 2025 giảm 8%
                if (NgayThem.HasValue && NgayThem.Value.Year == 2025)
                {
                    return 8;
                }

                // Sản phẩm thêm năm 2026 giảm 5%
                if (NgayThem.HasValue && NgayThem.Value.Year == 2026)
                {
                    return 5;
                }

                return 0;
            }
        }
    }
}