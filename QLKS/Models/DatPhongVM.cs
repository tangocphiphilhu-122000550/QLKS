using System;
using System.Collections.Generic;

namespace QLKS.Models
{
    public class DatPhongVM
    {
        public int MaDatPhong { get; set; }
        public int? MaNv { get; set; }
        public int? MaKh { get; set; }
        public string TenKhachHang { get; set; }
        public string MaPhong { get; set; }
        public DateOnly NgayDat { get; set; }
        public DateTime? NgayNhanPhong { get; set; }
        public DateTime? NgayTraPhong { get; set; }
        public int SoNguoiO { get; set; }
        public decimal? PhuThu { get; set; }
        public string TrangThai { get; set; }
        public decimal? TongTienPhong { get; set; }
        public int SoLuongDichVuSuDung { get; set; }
        public List<SuDungDichVuVM> DanhSachDichVu { get; set; } = new List<SuDungDichVuVM>();
        public List<TenKhachHangVM> DanhSachKhachHang { get; set; } = new List<TenKhachHangVM>(); // Sửa từ KhachHangMD sang TenKhachHangVM
    }

    public class CreateDatPhongVM
    {
        public int? MaNv { get; set; }
        public int? MaKh { get; set; }
        public string MaPhong { get; set; }
        public DateOnly? NgayDat { get; set; }
        public DateTime NgayNhanPhong { get; set; } // Đổi sang DateTime
        public DateTime NgayTraPhong { get; set; }  // Đổi sang DateTime
        public int SoNguoiO { get; set; }
        public string? TrangThai { get; set; }
    }


    public class UpdateDatPhongVM
    {
        public int? MaNv { get; set; }
        public int? MaKh { get; set; }
        public string MaPhong { get; set; }
        public DateOnly? NgayDat { get; set; }
        public DateTime? NgayNhanPhong { get; set; }
        public DateTime? NgayTraPhong { get; set; }
        public int SoNguoiO { get; set; }
        public string TrangThai { get; set; }
    }
    public class UpdatePhongTrangThaiVM
    {
        public string MaPhong { get; set; }
        public string TrangThai { get; set; }
    }

}