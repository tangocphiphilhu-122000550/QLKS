using QLKS.Data;

namespace QLKS.Models
{
    public class DichVuVM
    {
        public string TenDichVu { get; set; } = null!;
        public decimal DonGia { get; set; }
        public string? MoTa { get; set; }
    }
    public class DichVuMD : DichVuVM
    {
        public int MaDichVu { get; set; }
        public virtual ICollection<SuDungDichVu> SuDungDichVus { get; set; } = new List<SuDungDichVu>();
    }

    public class DichVuDTO
    {
        public string TenDichVu { get; set; } = null!;
        public decimal DonGia { get; set; }
        public string? MoTa { get; set; }
    }

    public class SuDungDichVuVM
    {
        public int? MaDatPhong { get; set; }
        public int? MaDichVu { get; set; }
        public int SoLuong { get; set; }
        public DateTime? NgaySuDung { get; set; } // Changed from DateOnly to DateTime
        public DateTime? NgayKetThuc { get; set; } // Changed from DateOnly to DateTime
        public decimal? ThanhTien { get; set; }
    }

    public class SuDungDichVu
    {
        public int MaSuDung { get; set; }
        public int? MaDatPhong { get; set; }
        public int? MaDichVu { get; set; }
        public int SoLuong { get; set; }
        public DateTime? NgaySuDung { get; set; } // Changed from DateOnly to DateTime
        public DateTime? NgayKetThuc { get; set; } // Changed from DateOnly to DateTime
        public decimal? ThanhTien { get; set; }
        public virtual DatPhong? MaDatPhongNavigation { get; set; }
        public virtual DichVu? MaDichVuNavigation { get; set; }
    }

}
