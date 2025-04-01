using System;
using System.Collections.Generic;

namespace QLKS.Data;

public partial class KhachHang
{
    public int MaKh { get; set; }

    public string HoTen { get; set; } = null!;

    public string? CccdPassport { get; set; }

    public string? SoDienThoai { get; set; }

    public string? QuocTich { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
}
