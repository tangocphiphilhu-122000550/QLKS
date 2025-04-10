// HoaDonVM.cs
using System;
using System.Collections.Generic;

namespace QLKS.Models
{
    public class HoaDonVM
    {
        public int MaHoaDon { get; set; }
        public int? MaKh { get; set; }
        public string TenKhachHang { get; set; }
        public int? MaNv { get; set; }
        public string TenNhanVien { get; set; }
        public DateOnly? NgayLap { get; set; }
        public decimal? TongTien { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThai { get; set; }
        public List<ChiTietHoaDonVM> ChiTietHoaDons { get; set; }
    }

    public class CreateHoaDonVM
    {
        public int? MaKh { get; set; }
        public int? MaNv { get; set; }
        public DateOnly? NgayLap { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThai { get; set; }
        public List<int> MaDatPhongs { get; set; }
    }

    public class UpdateHoaDonVM
    {
        public int? MaKh { get; set; }
        public int? MaNv { get; set; }
        public DateOnly? NgayLap { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThai { get; set; }
    }

    public class ChiTietHoaDonVM
    {
        public int? MaChiTietHoaDon { get; set; } // Sửa thành int? để khớp với ChiTietHoaDon
        public int? MaHoaDon { get; set; } // Sửa thành int? để khớp với ChiTietHoaDon
        public int? MaDatPhong { get; set; } // Sửa thành int? để khớp với ChiTietHoaDon
        public decimal? TongTienPhong { get; set; }
        public decimal? PhuThu { get; set; }
        public decimal? TongTienDichVu { get; set; }
        public List<SuDungDichVuVM> DanhSachDichVu { get; set; }
    }

}
