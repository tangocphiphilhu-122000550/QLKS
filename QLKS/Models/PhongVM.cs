using QLKS.Data;

namespace QLKS.Models
{
    public class PhongVM
    {

        public int? MaLoaiPhong { get; set; }

        public string? TenPhong { get; set; }

        public string? TrangThai { get; set; }
    }
    public class PhongMD : PhongVM
    {
        public string MaPhong { get; set; } = null!;
    }
}