using QLKS.Data;

namespace QLKS.Models 
{
    public class DatPhongVM
    {
        public int MaDatPhong { get; set; }

        // --- Mã khóa ngoại (Foreign Key IDs) ---
        public int? MaNv { get; set; } // Mã nhân viên
        public int? MaKh { get; set; } // Mã khách hàng
        public string? MaPhong { get; set; } // Mã của phòng

        // --- Chi tiết Đặt phòng ---
        public DateOnly? NgayDat { get; set; } // Ngày đặt phòng
        public DateOnly NgayNhanPhong { get; set; } // Ngày nhận phòng
        public DateOnly NgayTraPhong { get; set; } // Ngày trả phòng
        public int SoNguoiO { get; set; } // Số người ở
        public decimal? PhuThu { get; set; } // Phụ thu (nếu có)
        public string? TrangThai { get; set; } // Trạng thái đặt phòng (ví dụ: Đã xác nhận, Đã nhận phòng, Đã trả phòng)
        public decimal? TongTienPhong { get; set; } // Tổng tiền phòng (thường được tính toán sau)
        public int SoLuongDichVuSuDung { get; set; }
    }
}