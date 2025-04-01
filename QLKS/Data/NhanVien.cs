using System;
using System.Collections.Generic;

namespace QLKS.Data;

public partial class NhanVien
{
    public int MaNv { get; set; }

    public string HoTen { get; set; } = null!;

    public byte[] MatKhau { get; set; } = null!;

    public int? MaVaiTro { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? GioiTinh { get; set; }

    public string? DiaChi { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual VaiTro? MaVaiTroNavigation { get; set; }
}
