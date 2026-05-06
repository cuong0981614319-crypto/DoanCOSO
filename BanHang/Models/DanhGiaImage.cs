namespace BanHang.Models
{
    public class DanhGiaImage
    {
        public int Id { get; set; }
        public int DanhGiaId { get; set; }
        public string ImageUrl { get; set; }

        public DanhGia DanhGia { get; set; }
    }
}
