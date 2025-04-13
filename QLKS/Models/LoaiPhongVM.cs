namespace QLKS.Models
{
    // ViewModel cơ bản cho loại phòng
    public class LoaiPhongVM
    {
        public string TenLoaiPhong { get; set; } = null!;
        public decimal GiaCoBan { get; set; }
        public int SoNguoiToiDa { get; set; }
    }

    // Model đầy đủ với khóa chính
    public class LoaiPhongMD : LoaiPhongVM
    {
        public int MaLoaiPhong { get; set; }
    }
}