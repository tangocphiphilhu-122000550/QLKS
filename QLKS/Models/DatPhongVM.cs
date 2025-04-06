using System;
using System.Collections.Generic;

namespace QLKS.Models
{
    public class DatPhongVM
    {
        public int MaDatPhong { get; set; }
        public int? MaNv { get; set; }
        public int? MaKh { get; set; }
        public string? TenKhachHang { get; set; } // Thêm tên khách hàng
        public string MaPhong { get; set; }
        public DateOnly NgayDat { get; set; }
        public DateOnly NgayNhanPhong { get; set; }
        public DateOnly NgayTraPhong { get; set; }
        public int SoNguoiO { get; set; }
        public decimal? PhuThu { get; set; }
        public string? TrangThai { get; set; }
        public decimal? TongTienPhong { get; set; }
        public int SoLuongDichVuSuDung { get; set; }
        public List<SuDungDichVuVM> DanhSachDichVu { get; set; }
    }

    public class CreateDatPhongVM
    {
        public int? MaNv { get; set; }
        public int? MaKh { get; set; }
        public string MaPhong { get; set; }
        public DateOnly? NgayDat { get; set; }
        public DateOnly NgayNhanPhong { get; set; }
        public DateOnly NgayTraPhong { get; set; }
        public int SoNguoiO { get; set; }
        public string? TrangThai { get; set; }
        public decimal? TongTienPhong { get; set; }
        public List<SuDungDichVuVM>? DanhSachDichVu { get; set; }
    }

    public class UpdateDatPhongVM
    {
        public int? MaNv { get; set; }
        public int? MaKh { get; set; }
        public string MaPhong { get; set; }
        public DateOnly? NgayDat { get; set; }
        public DateOnly NgayNhanPhong { get; set; }
        public DateOnly NgayTraPhong { get; set; }
        public int SoNguoiO { get; set; }
        public decimal? PhuThu { get; set; }
        public string? TrangThai { get; set; }
        public decimal? TongTienPhong { get; set; }
    }
}