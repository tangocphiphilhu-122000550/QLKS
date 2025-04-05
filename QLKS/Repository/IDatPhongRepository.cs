using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS.Repository
{
    public interface IDatPhongRepository
    {
        Task<DatPhong> AddAsync(DatPhong datPhong);
        Task<DatPhong> UpdateAsync(DatPhong datPhong);
        Task<bool> DeleteAsync(int maDatPhong);
        Task<IEnumerable<DatPhong>> GetAllAsync();
        Task<DatPhong?> GetByIdAsync(int maDatPhong);
        Task<IEnumerable<DatPhong>> GetByMaKhAsync(int maKh);
        Task<IEnumerable<DatPhong>> GetByMaNvAsync(int maNv);
        Task<IEnumerable<DatPhong>> GetByMaPhongAsync(string maPhong);
        Task<IEnumerable<DatPhong>> GetByTrangThaiAsync(string trangThai);
        Task<bool> IsPhongDatAsync(string maPhong, DateOnly ngayNhanPhong, DateOnly ngayTraPhong);
    }

    public class DatPhongRepository : IDatPhongRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public DatPhongRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public async Task<DatPhong> AddAsync(DatPhong datPhong)
        {
            if (datPhong == null)
                throw new ArgumentNullException(nameof(datPhong));

            if (string.IsNullOrEmpty(datPhong.MaPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            if (datPhong.NgayNhanPhong == default || datPhong.NgayTraPhong == default)
                throw new ArgumentException("Ngày nhận phòng và ngày trả phòng không được để trống.");

            if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
                throw new ArgumentException("Ngày nhận phòng phải trước ngày trả phòng.");

            var phong = await _context.Phongs.FindAsync(datPhong.MaPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

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

            var validTrangThai = new[] { "Chờ xác nhận", "Đã xác nhận", "Hủy", "Hoàn thành" };
            if (!string.IsNullOrEmpty(datPhong.TrangThai) && !validTrangThai.Contains(datPhong.TrangThai))
                throw new ArgumentException("Trạng thái không hợp lệ.");

            _context.DatPhongs.Add(datPhong);
            await _context.SaveChangesAsync();
            return datPhong;
        }

        public async Task<DatPhong> UpdateAsync(DatPhong datPhong)
        {
            if (datPhong == null)
                throw new ArgumentNullException(nameof(datPhong));

            var existingDatPhong = await _context.DatPhongs.FindAsync(datPhong.MaDatPhong);
            if (existingDatPhong == null)
                throw new ArgumentException("Đặt phòng không tồn tại.");

            if (string.IsNullOrEmpty(datPhong.MaPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            if (datPhong.NgayNhanPhong == default || datPhong.NgayTraPhong == default)
                throw new ArgumentException("Ngày nhận phòng và ngày trả phòng không được để trống.");

            if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
                throw new ArgumentException("Ngày nhận phòng phải trước ngày trả phòng.");

            var phong = await _context.Phongs.FindAsync(datPhong.MaPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

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

            var validTrangThai = new[] { "Chờ xác nhận", "Đã xác nhận", "Hủy", "Hoàn thành" };
            if (!string.IsNullOrEmpty(datPhong.TrangThai) && !validTrangThai.Contains(datPhong.TrangThai))
                throw new ArgumentException("Trạng thái không hợp lệ.");

            _context.Entry(existingDatPhong).CurrentValues.SetValues(datPhong);
            await _context.SaveChangesAsync();
            return datPhong;
        }

        public async Task<bool> DeleteAsync(int maDatPhong)
        {
            var datPhong = await _context.DatPhongs.FindAsync(maDatPhong);
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

            _context.DatPhongs.Remove(datPhong);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DatPhong>> GetAllAsync()
        {
            return await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation) // Tải thông tin dịch vụ
                .ToListAsync();
        }

        public async Task<DatPhong?> GetByIdAsync(int maDatPhong)
        {
            return await _context.DatPhongs
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);
        }

        public async Task<IEnumerable<DatPhong>> GetByMaKhAsync(int maKh)
        {
            if (maKh <= 0)
                throw new ArgumentException("Mã khách hàng không hợp lệ.");

            var khachHang = await _context.KhachHangs.FindAsync(maKh);
            if (khachHang == null)
                throw new ArgumentException("Khách hàng không tồn tại.");

            return await _context.DatPhongs
                .Where(dp => dp.MaKh == maKh)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();
        }

        public async Task<IEnumerable<DatPhong>> GetByMaNvAsync(int maNv)
        {
            if (maNv <= 0)
                throw new ArgumentException("Mã nhân viên không hợp lệ.");

            var nhanVien = await _context.NhanViens.FindAsync(maNv);
            if (nhanVien == null)
                throw new ArgumentException("Nhân viên không tồn tại.");

            return await _context.DatPhongs
                .Where(dp => dp.MaNv == maNv)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();
        }

        public async Task<IEnumerable<DatPhong>> GetByMaPhongAsync(string maPhong)
        {
            if (string.IsNullOrWhiteSpace(maPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            var phong = await _context.Phongs.FindAsync(maPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

            return await _context.DatPhongs
                .Where(dp => dp.MaPhong == maPhong)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();
        }

        public async Task<IEnumerable<DatPhong>> GetByTrangThaiAsync(string trangThai)
        {
            if (string.IsNullOrWhiteSpace(trangThai))
                throw new ArgumentException("Trạng thái không được để trống.");

            var validTrangThai = new[] { "Chờ xác nhận", "Đã xác nhận", "Hủy", "Hoàn thành" };
            if (!validTrangThai.Contains(trangThai))
                throw new ArgumentException("Trạng thái không hợp lệ.");

            return await _context.DatPhongs
                .Where(dp => dp.TrangThai == trangThai)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();
        }

        public async Task<bool> IsPhongDatAsync(string maPhong, DateOnly ngayNhanPhong, DateOnly ngayTraPhong)
        {
            if (string.IsNullOrWhiteSpace(maPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            if (ngayNhanPhong == default || ngayTraPhong == default)
                throw new ArgumentException("Ngày nhận phòng và ngày trả phòng không được để trống.");

            if (ngayNhanPhong > ngayTraPhong)
                throw new ArgumentException("Ngày nhận phòng phải trước ngày trả phòng.");

            var ngayNhanPhongDateTime = ngayNhanPhong.ToDateTime(TimeOnly.MinValue);
            var ngayTraPhongDateTime = ngayTraPhong.ToDateTime(TimeOnly.MaxValue);

            return await _context.DatPhongs
                .Where(dp => dp.MaPhong == maPhong && dp.TrangThai != "Hủy")
                .AnyAsync(dp =>
                    (ngayNhanPhongDateTime >= dp.NgayNhanPhong.ToDateTime(TimeOnly.MinValue) && ngayNhanPhongDateTime < dp.NgayTraPhong.ToDateTime(TimeOnly.MaxValue)) ||
                    (ngayTraPhongDateTime > dp.NgayNhanPhong.ToDateTime(TimeOnly.MinValue) && ngayTraPhongDateTime <= dp.NgayTraPhong.ToDateTime(TimeOnly.MaxValue)) ||
                    (ngayNhanPhongDateTime <= dp.NgayNhanPhong.ToDateTime(TimeOnly.MinValue) && ngayTraPhongDateTime >= dp.NgayTraPhong.ToDateTime(TimeOnly.MaxValue)));
        }
    }
}