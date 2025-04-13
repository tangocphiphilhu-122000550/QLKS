using QLKS.Data;

namespace QLKS.Models
{
    // ViewModel cơ bản
    public class KhachHangVM
    {
        public int? MaDatPhong { get; set; }
        public string HoTen { get; set; } = null!;
        public string? CccdPassport { get; set; }
        public string? SoDienThoai { get; set; }
        public string? QuocTich { get; set; }
        public string? GhiChu { get; set; }

    }

    // Model đầy đủ với khóa chính và navigation property
    public class KhachHangMD : KhachHangVM
    {
        public int MaKh { get; set; }
    }

    // DTO để truyền dữ liệu
    public class KhachHangDTO
    {
        public string HoTen { get; set; } = null!;
        public string? CccdPassport { get; set; }
        public string? SoDienThoai { get; set; }
        public string? QuocTich { get; set; }
        public string? GhiChu { get; set; }
    }

    public class TenKhachHangVM
    {
        public string HoTen { get; set; } = null!;
    }
}