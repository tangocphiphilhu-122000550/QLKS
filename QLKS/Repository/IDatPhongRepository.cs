using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS.Repository
{
    public interface IDatPhongRepository
    {
        Task<IEnumerable<DatPhongVM>> GetAllVMAsync();
        Task<IEnumerable<DatPhongVM>> GetByMaPhongVMAsync(string maPhong);
        Task<IEnumerable<DatPhongVM>> GetByTenKhachHangVMAsync(string tenKhachHang);
        Task<DatPhongVM> AddVMAsync(CreateDatPhongVM datPhongVM);
        Task<DatPhongVM> UpdateVMAsync(int maDatPhong, UpdateDatPhongVM datPhongVM);
        Task<bool> DeleteByMaDatPhongAsync(int maDatPhong);
    }

    public class DatPhongRepository : IDatPhongRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public DatPhongRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DatPhongVM>> GetAllVMAsync()
        {
            var datPhongs = await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Include(dp => dp.MaKhNavigation)
                .ToListAsync();
            return datPhongs.Select(dp => MapToVM(dp));
        }

        public async Task<IEnumerable<DatPhongVM>> GetByMaPhongVMAsync(string maPhong)
        {
            if (string.IsNullOrWhiteSpace(maPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            var phong = await _context.Phongs.FindAsync(maPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

            var datPhongs = await _context.DatPhongs
                .Where(dp => dp.MaPhong == maPhong)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Include(dp => dp.MaKhNavigation)
                .ToListAsync();
            return datPhongs.Select(dp => MapToVM(dp));
        }

        public async Task<IEnumerable<DatPhongVM>> GetByTenKhachHangVMAsync(string tenKhachHang)
        {
            if (string.IsNullOrWhiteSpace(tenKhachHang))
                throw new ArgumentException("Tên khách hàng không được để trống.");

            var datPhongs = await _context.DatPhongs
                .Include(dp => dp.MaKhNavigation)
                .Where(dp => dp.MaKhNavigation.HoTen.Contains(tenKhachHang))
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();
            return datPhongs.Select(dp => MapToVM(dp));
        }

        public async Task<DatPhongVM> AddVMAsync(CreateDatPhongVM datPhongVM)
        {
            if (datPhongVM == null)
                throw new ArgumentNullException(nameof(datPhongVM));

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(datPhongVM.MaPhong))
                throw new ArgumentException("Mã phòng không được để trống.");
            if (datPhongVM.NgayNhanPhong == default || datPhongVM.NgayTraPhong == default)
                throw new ArgumentException("Ngày nhận phòng và ngày trả phòng không được để trống.");

            var datPhong = new DatPhong
            {
                MaNv = datPhongVM.MaNv,
                MaKh = datPhongVM.MaKh,
                MaPhong = datPhongVM.MaPhong,
                NgayDat = datPhongVM.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                NgayNhanPhong = datPhongVM.NgayNhanPhong,
                NgayTraPhong = datPhongVM.NgayTraPhong,
                SoNguoiO = datPhongVM.SoNguoiO,
                TrangThai = "Đang sử dụng" // Mặc định là "Đang sử dụng"
            };

            datPhong.PhuThu = await CalculatePhuThu(datPhongVM.MaPhong, datPhongVM.SoNguoiO, datPhongVM.NgayNhanPhong, datPhongVM.NgayTraPhong);
            datPhong.TongTienPhong = await CalculateTongTienPhong(datPhongVM.MaPhong, datPhongVM.NgayNhanPhong, datPhongVM.NgayTraPhong, datPhong.PhuThu);

            await ValidateDatPhong(datPhong);
            _context.DatPhongs.Add(datPhong);

            // Cập nhật trạng thái phòng
            await UpdatePhongTrangThai(datPhong.MaPhong, datPhong.TrangThai);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message}", ex);
            }

            if (datPhongVM.DanhSachDichVu != null && datPhongVM.DanhSachDichVu.Any())
            {
                foreach (var dv in datPhongVM.DanhSachDichVu)
                {
                    if (!await _context.DichVus.AnyAsync(d => d.MaDichVu == dv.MaDichVu))
                        throw new ArgumentException($"Dịch vụ với MaDichVu {dv.MaDichVu} không tồn tại.");

                    var suDungDichVu = new Data.SuDungDichVu
                    {
                        MaDatPhong = datPhong.MaDatPhong,
                        MaDichVu = dv.MaDichVu,
                        SoLuong = dv.SoLuong,
                        NgaySuDung = dv.NgaySuDung.HasValue ? DateOnly.FromDateTime(dv.NgaySuDung.Value) : null,
                        NgayKetThuc = dv.NgayKetThuc.HasValue ? DateOnly.FromDateTime(dv.NgayKetThuc.Value) : null,
                        ThanhTien = dv.ThanhTien
                    };
                    _context.SuDungDichVus.Add(suDungDichVu);
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new Exception($"Lỗi khi lưu dịch vụ: {ex.InnerException?.Message}", ex);
                }
            }

            var newDp = await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Include(dp => dp.MaKhNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == datPhong.MaDatPhong);

            if (newDp == null)
                throw new Exception("Không thể truy xuất đặt phòng vừa tạo.");

            return MapToVM(newDp);
        }

        public async Task<DatPhongVM> UpdateVMAsync(int maDatPhong, UpdateDatPhongVM datPhongVM)
        {
            var existingDatPhong = await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Include(dp => dp.MaKhNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (existingDatPhong == null)
                throw new ArgumentException("Đặt phòng không tồn tại.");

            var datPhong = new DatPhong
            {
                MaDatPhong = maDatPhong,
                MaNv = datPhongVM.MaNv,
                MaKh = datPhongVM.MaKh,
                MaPhong = datPhongVM.MaPhong,
                NgayDat = datPhongVM.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                NgayNhanPhong = datPhongVM.NgayNhanPhong,
                NgayTraPhong = datPhongVM.NgayTraPhong,
                SoNguoiO = datPhongVM.SoNguoiO,
                TrangThai = datPhongVM.TrangThai?.Trim() // Chuẩn hóa giá trị
            };

            datPhong.PhuThu = await CalculatePhuThu(datPhongVM.MaPhong, datPhongVM.SoNguoiO, datPhongVM.NgayNhanPhong, datPhongVM.NgayTraPhong);
            datPhong.TongTienPhong = await CalculateTongTienPhong(datPhongVM.MaPhong, datPhongVM.NgayNhanPhong, datPhongVM.NgayTraPhong, datPhong.PhuThu);

            await ValidateDatPhong(datPhong);
            _context.Entry(existingDatPhong).CurrentValues.SetValues(datPhong);

            try
            {
                Console.WriteLine($"Cập nhật DatPhong.TrangThai: '{datPhong.TrangThai}'");
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi cập nhật dữ liệu: {ex.InnerException?.Message}", ex);
            }

            await UpdatePhongTrangThai(datPhong.MaPhong, datPhong.TrangThai);

            var updatedDp = await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Include(dp => dp.MaKhNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (updatedDp == null)
                throw new Exception("Không thể truy xuất đặt phòng vừa cập nhật.");

            return MapToVM(updatedDp);
        }

        public async Task<bool> DeleteByMaDatPhongAsync(int maDatPhong)
        {
            var datPhong = await _context.DatPhongs
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (datPhong == null)
                return false;

            var suDungDichVus = await _context.SuDungDichVus
                .Where(sddv => sddv.MaDatPhong == maDatPhong)
                .ToListAsync();
            if (suDungDichVus.Any())
            {
                _context.SuDungDichVus.RemoveRange(suDungDichVus);
            }

            var chiTietHoaDons = await _context.ChiTietHoaDons
                .Where(cthd => cthd.MaDatPhong == maDatPhong)
                .ToListAsync();
            if (chiTietHoaDons.Any())
            {
                _context.ChiTietHoaDons.RemoveRange(chiTietHoaDons);
            }

            string maPhong = datPhong.MaPhong;
            _context.DatPhongs.Remove(datPhong);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi xóa dữ liệu: {ex.InnerException?.Message}", ex);
            }

            await UpdatePhongTrangThaiAfterDelete(maPhong);
            return true;
        }

        private async Task ValidateDatPhong(DatPhong datPhong)
        {
            if (string.IsNullOrEmpty(datPhong.MaPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            if (datPhong.NgayNhanPhong == default || datPhong.NgayTraPhong == default)
                throw new ArgumentException("Ngày nhận phòng và ngày trả phòng không được để trống.");

            if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
                throw new ArgumentException("Ngày nhận phòng phải trước ngày trả phòng.");

            var phong = await _context.Phongs.FindAsync(datPhong.MaPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

            // Chặn đặt phòng nếu phòng đang "Bảo trì" (trừ trạng thái "Hủy")
            if (phong.TrangThai == "Bảo trì" && datPhong.TrangThai != "Hủy")
                throw new ArgumentException("Phòng đang bảo trì, không thể đặt hoặc sử dụng.");

            if (datPhong.MaKh.HasValue)
            {
                var khachHang = await _context.KhachHangs.FindAsync(datPhong.MaKh);
                if (khachHang == null)
                    throw new ArgumentException("Khách hàng không tồn tại.");
            }

            if (datPhong.MaNv.HasValue)
            {
                var nhanVien = await _context.NhanViens.FindAsync(datPhong.MaNv);
                if (nhanVien == null)
                    throw new ArgumentException("Nhân viên không tồn tại.");
            }

            var validTrangThai = new[] { "Đang sử dụng", "Hủy", "Hoàn thành" };
            if (!string.IsNullOrEmpty(datPhong.TrangThai) && !validTrangThai.Contains(datPhong.TrangThai))
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ cho phép: Đang sử dụng, Hủy, Hoàn thành.");
        }

        private async Task<decimal?> CalculatePhuThu(string maPhong, int soNguoiO, DateOnly ngayNhanPhong, DateOnly ngayTraPhong)
        {
            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null || phong.MaLoaiPhong == null)
                return 0;

            var phuThu = await _context.PhuThus
                .FirstOrDefaultAsync(pt => pt.MaLoaiPhong == phong.MaLoaiPhong);

            if (phuThu == null || phuThu.PhuThuNguoiThem == null)
                return 0;

            var loaiPhong = phong.MaLoaiPhongNavigation;
            int soNguoiToiDa = loaiPhong?.SoNguoiToiDa ?? 2;
            int soNguoiThem = Math.Max(0, soNguoiO - soNguoiToiDa);
            int soNgayO = ngayTraPhong.DayNumber - ngayNhanPhong.DayNumber;

            return soNguoiThem * phuThu.PhuThuNguoiThem * soNgayO;
        }

        private async Task<decimal> CalculateTongTienPhong(string maPhong, DateOnly ngayNhanPhong, DateOnly ngayTraPhong, decimal? phuThu)
        {
            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null || phong.MaLoaiPhongNavigation == null)
                return 0;

            decimal giaPhong = phong.MaLoaiPhongNavigation.GiaCoBan;
            int soNgayO = ngayTraPhong.DayNumber - ngayNhanPhong.DayNumber;

            return (giaPhong * soNgayO) + (phuThu ?? 0);
        }

        private async Task UpdatePhongTrangThai(string maPhong, string datPhongTrangThai)
        {
            var phong = await _context.Phongs.FirstOrDefaultAsync(p => p.MaPhong == maPhong);
            if (phong == null)
                throw new Exception($"Phòng {maPhong} không tồn tại.");

            // Không thay đổi trạng thái nếu phòng đang "Bảo trì"
            if (phong.TrangThai == "Bảo trì")
            {
                return; // Giữ nguyên trạng thái "Bảo trì"
            }

            // Cập nhật trạng thái phòng dựa trên trạng thái đặt phòng
            if (datPhongTrangThai == "Đang sử dụng")
            {
                phong.TrangThai = "Đang sử dụng";
            }
            else if (datPhongTrangThai == "Hủy" || datPhongTrangThai == "Hoàn thành")
            {
                var activeBookings = await _context.DatPhongs
                    .AnyAsync(dp => dp.MaPhong == maPhong && dp.TrangThai == "Đang sử dụng");
                phong.TrangThai = activeBookings ? "Đang sử dụng" : "Trống";
            }
            // Không xử lý "Đã đặt" hoặc các trạng thái khác

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái phòng: {ex.InnerException?.Message}", ex);
            }
        }

        private async Task UpdatePhongTrangThaiAfterDelete(string maPhong)
        {
            var phong = await _context.Phongs.FirstOrDefaultAsync(p => p.MaPhong == maPhong);
            if (phong == null)
                return;

            // Không thay đổi trạng thái nếu phòng đang "Bảo trì"
            if (phong.TrangThai == "Bảo trì")
                return;

            var activeBookings = await _context.DatPhongs
                .AnyAsync(dp => dp.MaPhong == maPhong && dp.TrangThai == "Đang sử dụng");
            phong.TrangThai = activeBookings ? "Đang sử dụng" : "Trống";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái phòng: {ex.InnerException?.Message}", ex);
            }
        }

        private DatPhongVM MapToVM(DatPhong dp)
        {
            return new DatPhongVM
            {
                MaDatPhong = dp.MaDatPhong,
                MaNv = dp.MaNv,
                MaKh = dp.MaKh,
                TenKhachHang = dp.MaKhNavigation?.HoTen,
                MaPhong = dp.MaPhong,
                NgayDat = dp.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                NgayNhanPhong = dp.NgayNhanPhong,
                NgayTraPhong = dp.NgayTraPhong,
                SoNguoiO = dp.SoNguoiO,
                PhuThu = dp.PhuThu,
                TrangThai = dp.TrangThai,
                TongTienPhong = dp.TongTienPhong,
                SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0,
                DanhSachDichVu = dp.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                {
                    MaSuDung = sddv.MaSuDung,
                    MaDichVu = sddv.MaDichVu,
                    TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                    SoLuong = sddv.SoLuong,
                    NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    ThanhTien = sddv.ThanhTien
                }).ToList() ?? new List<SuDungDichVuVM>()
            };
        }
    }
}