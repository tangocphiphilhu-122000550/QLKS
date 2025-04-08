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
        Task<List<DatPhongVM>> GetAllVMAsync();
        Task<DatPhongVM> GetByIdVMAsync(int maDatPhong);
        Task AddVMAsync(CreateDatPhongVM datPhongVM);
        Task UpdateVMAsync(int maDatPhong, UpdateDatPhongVM datPhongVM);
        Task<bool> DeleteByMaDatPhongAsync(int maDatPhong);
    }
    public class DatPhongRepository : IDatPhongRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public DatPhongRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public async Task<List<DatPhongVM>> GetAllVMAsync()
        {
            var datPhongs = await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();

            return datPhongs.Select(dp => MapToVM(dp)).ToList();
        }

        public async Task<DatPhongVM> GetByIdVMAsync(int maDatPhong)
        {
            var datPhong = await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (datPhong == null)
                return null;

            return MapToVM(datPhong);
        }

        public async Task AddVMAsync(CreateDatPhongVM datPhongVM)
        {
            var datPhong = new DatPhong
            {
                MaNv = datPhongVM.MaNv,
                MaKh = datPhongVM.MaKh,
                MaPhong = datPhongVM.MaPhong,
                NgayDat = datPhongVM.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                NgayNhanPhong = datPhongVM.NgayNhanPhong,
                NgayTraPhong = datPhongVM.NgayTraPhong,
                SoNguoiO = datPhongVM.SoNguoiO,
                TrangThai = datPhongVM.TrangThai ?? "Đang sử dụng"
            };

            await ValidateDatPhong(datPhong);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Thêm bản ghi DatPhong
                    var sqlInsert = @"
                        INSERT INTO dbo.DatPhong (MaNv, MaKh, MaPhong, NgayDat, NgayNhanPhong, NgayTraPhong, SoNguoiO, TrangThai)
                        VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});";

                    // Xử lý giá trị null cho MaNv, MaKh, MaPhong, NgayDat, TrangThai
                    object maNvParam = datPhong.MaNv.HasValue ? (object)datPhong.MaNv.Value : DBNull.Value;
                    object maKhParam = datPhong.MaKh.HasValue ? (object)datPhong.MaKh.Value : DBNull.Value;
                    object maPhongParam = datPhong.MaPhong ?? (object)DBNull.Value;
                    object ngayDatParam = datPhong.NgayDat.HasValue ? (object)datPhong.NgayDat.Value : DBNull.Value;
                    object trangThaiParam = datPhong.TrangThai ?? (object)DBNull.Value;

                    await _context.Database.ExecuteSqlRawAsync(sqlInsert,
                        maNvParam,
                        maKhParam,
                        maPhongParam,
                        ngayDatParam,
                        datPhong.NgayNhanPhong,
                        datPhong.NgayTraPhong,
                        datPhong.SoNguoiO,
                        trangThaiParam);

                    // Trigger trg_DatPhong_Insert sẽ tự động tính PhuThu và TongTienPhong

                    await UpdatePhongTrangThai(datPhong.MaPhong, datPhong.TrangThai);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi thêm dữ liệu: {ex.Message}", ex);
                }
            }
        }

        public async Task UpdateVMAsync(int maDatPhong, UpdateDatPhongVM datPhongVM)
        {
            var existingDatPhong = await _context.DatPhongs
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (existingDatPhong == null)
                throw new ArgumentException("Đặt phòng không tồn tại.");

            // Cập nhật các trường từ UpdateDatPhongVM
            existingDatPhong.NgayDat = datPhongVM.NgayDat ?? existingDatPhong.NgayDat;
            existingDatPhong.NgayNhanPhong = datPhongVM.NgayNhanPhong;
            existingDatPhong.NgayTraPhong = datPhongVM.NgayTraPhong;
            existingDatPhong.SoNguoiO = datPhongVM.SoNguoiO;
            existingDatPhong.TrangThai = datPhongVM.TrangThai ?? existingDatPhong.TrangThai;

            await ValidateDatPhong(existingDatPhong);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Sử dụng ExecuteSqlRawAsync để UPDATE
                    var sql = @"
                        UPDATE dbo.DatPhong
                        SET NgayDat = {0}, NgayNhanPhong = {1}, NgayTraPhong = {2}, SoNguoiO = {3}, TrangThai = {4}
                        WHERE MaDatPhong = {5};";

                    await _context.Database.ExecuteSqlRawAsync(sql,
                        existingDatPhong.NgayDat,
                        existingDatPhong.NgayNhanPhong,
                        existingDatPhong.NgayTraPhong,
                        existingDatPhong.SoNguoiO,
                        existingDatPhong.TrangThai,
                        maDatPhong);

                    // Trigger trg_DatPhong_Update sẽ tự động cập nhật PhuThu và TongTienPhong

                    await UpdatePhongTrangThai(existingDatPhong.MaPhong, existingDatPhong.TrangThai);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi cập nhật dữ liệu: {ex.Message}", ex);
                }
            }
        }

        public async Task<bool> DeleteByMaDatPhongAsync(int maDatPhong)
        {
            var datPhong = await _context.DatPhongs
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (datPhong == null)
                return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var suDungDichVus = await _context.SuDungDichVus
                        .Where(sddv => sddv.MaDatPhong == maDatPhong)
                        .ToListAsync();
                    if (suDungDichVus.Any())
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "DELETE FROM dbo.SuDungDichVu WHERE MaDatPhong = {0}", maDatPhong);
                    }

                    var chiTietHoaDons = await _context.ChiTietHoaDons
                        .Where(cthd => cthd.MaDatPhong == maDatPhong)
                        .ToListAsync();
                    if (chiTietHoaDons.Any())
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "DELETE FROM dbo.ChiTietHoaDon WHERE MaDatPhong = {0}", maDatPhong);
                    }

                    string maPhong = datPhong.MaPhong;

                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM dbo.DatPhong WHERE MaDatPhong = {0}", maDatPhong);

                    await UpdatePhongTrangThaiAfterDelete(maPhong);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi xóa dữ liệu: {ex.Message}", ex);
                }
            }

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

            // Kiểm tra SoNguoiO (bắt buộc, không được để trống)
            if (datPhong.SoNguoiO <= 0)
                throw new ArgumentException("Số người ở phải lớn hơn 0.");

            var phong = await _context.Phongs.FindAsync(datPhong.MaPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

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

        private async Task UpdatePhongTrangThai(string maPhong, string trangThai)
        {
            if (string.IsNullOrEmpty(maPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            if (string.IsNullOrEmpty(trangThai))
                throw new ArgumentException("Trạng thái không được để trống.");

            string trangThaiPhong;
            switch (trangThai)
            {
                case "Đang sử dụng":
                    trangThaiPhong = "Đang sử dụng";
                    break;
                case "Hủy":
                case "Hoàn thành":
                    trangThaiPhong = "Trống";
                    break;
                default:
                    throw new ArgumentException("Trạng thái đặt phòng không hợp lệ.");
            }

            // Ensure the value complies with the CHECK constraint
            if (!new[] { "Bảo trì", "Đang sử dụng", "Đã đặt", "Trống" }.Contains(trangThaiPhong))
                throw new ArgumentException("Trạng thái phòng không hợp lệ theo ràng buộc CHECK.");

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE dbo.Phong SET TrangThai = {0} WHERE MaPhong = {1}",
                trangThaiPhong, maPhong);
        }

        private async Task UpdatePhongTrangThaiAfterDelete(string maPhong)
        {
            if (string.IsNullOrEmpty(maPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            // Ensure the value complies with the CHECK constraint
            string trangThaiPhong = "Trống";
            if (!new[] { "Bảo trì", "Đang sử dụng", "Đã đặt", "Trống" }.Contains(trangThaiPhong))
                throw new ArgumentException("Trạng thái phòng không hợp lệ theo ràng buộc CHECK.");

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE dbo.Phong SET TrangThai = {0} WHERE MaPhong = {1}", trangThaiPhong, maPhong);
        }

        private DatPhongVM MapToVM(DatPhong datPhong)
        {
            return new DatPhongVM
            {
                MaDatPhong = datPhong.MaDatPhong,
                MaNv = datPhong.MaNv,
                MaKh = datPhong.MaKh,
                TenKhachHang = datPhong.MaKhNavigation?.HoTen,
                MaPhong = datPhong.MaPhong,
                NgayDat = datPhong.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                NgayNhanPhong = datPhong.NgayNhanPhong ?? default(DateTime),
                NgayTraPhong = datPhong.NgayTraPhong ?? default(DateTime),
                SoNguoiO = datPhong.SoNguoiO,
                PhuThu = datPhong.PhuThu,
                TrangThai = datPhong.TrangThai,
                TongTienPhong = datPhong.TongTienPhong,
                SoLuongDichVuSuDung = datPhong.SuDungDichVus?.Count ?? 0,
                DanhSachDichVu = datPhong.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                {
                    MaSuDung = sddv.MaSuDung,
                    MaDatPhong = sddv.MaDatPhong,
                    MaDichVu = sddv.MaDichVu,
                    TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                    SoLuong = sddv.SoLuong,
                    ThanhTien = sddv.ThanhTien
                }).ToList() ?? new List<SuDungDichVuVM>()
            };
        }
    }
}