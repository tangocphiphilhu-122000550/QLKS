using QLKS.Data;

namespace QLKS.Models
{
    public class PhongVM
    {
        public int? MaLoaiPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? TrangThai { get; set; }
        public string? TenLoaiPhong { get; set; } // THAY ĐỔI: Thêm TenLoaiPhong
        public decimal GiaCoBan { get; set; }     // THAY ĐỔI: Thêm GiaCoBan
        public int SoNguoiToiDa { get; set; }     // THAY ĐỔI: Thêm SoNguoiToiDa
    }

    public class PhongMD : PhongVM
    {
        public string MaPhong { get; set; } = null!;
    }
}