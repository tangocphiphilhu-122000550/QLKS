using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS.Repository
{
    public interface IHoaDonRepository
    {
        Task<IEnumerable<HoaDonVM>> GetAllAsync();
        Task<HoaDonVM> GetByIdAsync(int maHoaDon);
        Task<IEnumerable<HoaDonVM>> GetByMaKhAsync(int maKh);
        Task<HoaDonVM> AddAsync(CreateHoaDonVM hoaDonVM);
        Task<HoaDonVM> UpdateAsync(int maHoaDon, UpdateHoaDonVM hoaDonVM);
        Task<bool> DeleteAsync(int maHoaDon);
        Task<HoaDonVM> ThanhToanAsync(int maHoaDon, string phuongThucThanhToan);
        Task<IEnumerable<HoaDonVM>> GetByTrangThaiAsync(string trangThai);
    }

    public class HoaDonRepository : IHoaDonRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public HoaDonRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HoaDonVM>> GetAllAsync()
        {
            var hoaDons = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();

            return hoaDons.Select(hd => MapToVM(hd));
        }

        public async Task<HoaDonVM> GetByIdAsync(int maHoaDon)
        {
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

            if (hoaDon == null)
                throw new ArgumentException("Hóa đơn không tồn tại.");

            return MapToVM(hoaDon);
        }

        public async Task<IEnumerable<HoaDonVM>> GetByMaKhAsync(int maKh)
        {
            var khachHang = await _context.KhachHangs.FindAsync(maKh);
            if (khachHang == null)
                throw new ArgumentException("Khách hàng không tồn tại.");

            var hoaDons = await _context.HoaDons
                .Where(hd => hd.MaKh == maKh)
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();

            return hoaDons.Select(hd => MapToVM(hd));
        }

        public async Task<HoaDonVM> AddAsync(CreateHoaDonVM hoaDonVM)
        {
            if (hoaDonVM == null)
                throw new ArgumentNullException(nameof(hoaDonVM));

            if (hoaDonVM.MaDatPhongs == null || !hoaDonVM.MaDatPhongs.Any())
                throw new ArgumentException("Danh sách đặt phòng không được để trống.");

            var hoaDon = new HoaDon
            {
                MaKh = hoaDonVM.MaKh,
                MaNv = hoaDonVM.MaNv,
                NgayLap = hoaDonVM.NgayLap ?? DateOnly.FromDateTime(DateTime.Now),
                PhuongThucThanhToan = hoaDonVM.PhuongThucThanhToan,
                TrangThai = hoaDonVM.TrangThai ?? "Chưa thanh toán"
            };

            await ValidateHoaDon(hoaDon);

            foreach (var maDatPhong in hoaDonVM.MaDatPhongs)
            {
                var datPhong = await _context.DatPhongs.FindAsync(maDatPhong);
                if (datPhong == null)
                    throw new ArgumentException($"Đặt phòng với MaDatPhong {maDatPhong} không tồn tại.");

                var existingChiTiet = await _context.ChiTietHoaDons
                    .AnyAsync(cthd => cthd.MaDatPhong == maDatPhong);
                if (existingChiTiet)
                    throw new ArgumentException($"Đặt phòng với MaDatPhong {maDatPhong} đã được gán cho hóa đơn khác.");

                hoaDon.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaDatPhong = maDatPhong
                });
            }

            hoaDon.TongTien = await CalculateTongTien(hoaDon);
            _context.HoaDons.Add(hoaDon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message}", ex);
            }

            var newHoaDon = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == hoaDon.MaHoaDon);

            if (newHoaDon == null)
                throw new Exception("Không thể truy xuất hóa đơn vừa tạo.");

            return MapToVM(newHoaDon);
        }

        public async Task<HoaDonVM> UpdateAsync(int maHoaDon, UpdateHoaDonVM hoaDonVM)
        {
            var existingHoaDon = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

            if (existingHoaDon == null)
                throw new ArgumentException("Hóa đơn không tồn tại.");

            var hoaDon = new HoaDon
            {
                MaHoaDon = maHoaDon,
                MaKh = hoaDonVM.MaKh,
                MaNv = hoaDonVM.MaNv,
                NgayLap = hoaDonVM.NgayLap ?? existingHoaDon.NgayLap,
                PhuongThucThanhToan = hoaDonVM.PhuongThucThanhToan,
                TrangThai = hoaDonVM.TrangThai ?? existingHoaDon.TrangThai
            };

            await ValidateHoaDon(hoaDon);
            hoaDon.TongTien = await CalculateTongTien(existingHoaDon);
            _context.Entry(existingHoaDon).CurrentValues.SetValues(hoaDon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi cập nhật dữ liệu: {ex.InnerException?.Message}", ex);
            }

            var updatedHoaDon = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

            if (updatedHoaDon == null)
                throw new Exception("Không thể truy xuất hóa đơn vừa cập nhật.");

            return MapToVM(updatedHoaDon);
        }

        public async Task<bool> DeleteAsync(int maHoaDon)
        {
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.ChiTietHoaDons)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

            if (hoaDon == null)
                return false;

            if (hoaDon.ChiTietHoaDons.Any())
            {
                _context.ChiTietHoaDons.RemoveRange(hoaDon.ChiTietHoaDons);
            }

            _context.HoaDons.Remove(hoaDon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi xóa dữ liệu: {ex.InnerException?.Message}", ex);
            }

            return true;
        }

        public async Task<HoaDonVM> ThanhToanAsync(int maHoaDon, string phuongThucThanhToan)
        {
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

            if (hoaDon == null)
                throw new ArgumentException("Hóa đơn không tồn tại.");

            if (hoaDon.TrangThai == "Đã thanh toán")
                throw new ArgumentException("Hóa đơn đã được thanh toán.");

            hoaDon.TrangThai = "Đã thanh toán";
            hoaDon.PhuongThucThanhToan = phuongThucThanhToan;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái thanh toán: {ex.InnerException?.Message}", ex);
            }

            return MapToVM(hoaDon);
        }

        private async Task<decimal> CalculateTongTien(HoaDon hoaDon)
        {
            decimal tongTien = 0;

            var chiTietHoaDons = await _context.ChiTietHoaDons
                .Where(cthd => cthd.MaHoaDon == hoaDon.MaHoaDon)
                .Include(cthd => cthd.MaDatPhongNavigation)
                    .ThenInclude(dp => dp.SuDungDichVus)
                .ToListAsync();

            foreach (var chiTiet in chiTietHoaDons)
            {
                var datPhong = chiTiet.MaDatPhongNavigation;
                if (datPhong != null)
                {
                    tongTien += (datPhong.TongTienPhong ?? 0) + (datPhong.PhuThu ?? 0);
                    if (datPhong.SuDungDichVus != null)
                    {
                        tongTien += datPhong.SuDungDichVus.Sum(sddv => sddv.ThanhTien ?? 0);
                    }
                }
            }

            return tongTien;
        }

        private async Task ValidateHoaDon(HoaDon hoaDon)
        {
            if (hoaDon.MaKh.HasValue)
            {
                var khachHang = await _context.KhachHangs.FindAsync(hoaDon.MaKh);
                if (khachHang == null)
                    throw new ArgumentException("Khách hàng không tồn tại.");
            }

            if (hoaDon.MaNv.HasValue)
            {
                var nhanVien = await _context.NhanViens.FindAsync(hoaDon.MaNv);
                if (nhanVien == null)
                    throw new ArgumentException("Nhân viên không tồn tại.");
            }

            var validTrangThai = new[] { "Chưa thanh toán", "Đã thanh toán" };
            if (!string.IsNullOrEmpty(hoaDon.TrangThai) && !validTrangThai.Contains(hoaDon.TrangThai))
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ cho phép: Chưa thanh toán, Đã thanh toán.");
        }

        // Trong HoaDonRepository.cs, sửa MapToVM
        private HoaDonVM MapToVM(HoaDon hd)
        {
            Console.WriteLine($"Ánh xạ hóa đơn: MaHoaDon = {hd.MaHoaDon}, Số lượng ChiTietHoaDon = {hd.ChiTietHoaDons?.Count ?? 0}");
            return new HoaDonVM
            {
                MaHoaDon = hd.MaHoaDon,
                MaKh = hd.MaKh,
                TenKhachHang = hd.MaKhNavigation?.HoTen,
                MaNv = hd.MaNv,
                TenNhanVien = hd.MaNvNavigation?.HoTen,
                NgayLap = hd.NgayLap,
                TongTien = hd.TongTien,
                PhuongThucThanhToan = hd.PhuongThucThanhToan,
                TrangThai = hd.TrangThai,
                ChiTietHoaDons = hd.ChiTietHoaDons?.Select(cthd =>
                {
                    Console.WriteLine($"Ánh xạ ChiTietHoaDon: MaChiTietHoaDon = {cthd.MaChiTietHoaDon}, MaDatPhong = {cthd.MaDatPhong}");
                    return new ChiTietHoaDonVM
                    {
                        MaChiTietHoaDon = cthd.MaChiTietHoaDon,
                        MaHoaDon = cthd.MaHoaDon,
                        MaDatPhong = cthd.MaDatPhong,
                        TongTienPhong = cthd.MaDatPhongNavigation?.TongTienPhong,
                        PhuThu = cthd.MaDatPhongNavigation?.PhuThu,
                        DanhSachDichVu = cthd.MaDatPhongNavigation?.SuDungDichVus?.Select(sddv =>
                        {
                            Console.WriteLine($"Ánh xạ SuDungDichVu: MaSuDung = {sddv.MaSuDung}, MaDichVu = {sddv.MaDichVu}");
                            return new SuDungDichVuVM
                            {
                                MaSuDung = sddv.MaSuDung,
                                MaDichVu = sddv.MaDichVu,
                                TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                                SoLuong = sddv.SoLuong,
                                NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                                NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                                ThanhTien = sddv.ThanhTien
                            };
                        }).ToList() ?? new List<SuDungDichVuVM>()
                    };
                }).ToList() ?? new List<ChiTietHoaDonVM>()
            };
        }
        public async Task<IEnumerable<HoaDonVM>> GetByTrangThaiAsync(string trangThai)
        {
            if (string.IsNullOrWhiteSpace(trangThai))
                throw new ArgumentException("Trạng thái không được để trống.");

            var validTrangThai = new[] { "Chưa thanh toán", "Đã thanh toán" };
            if (!validTrangThai.Contains(trangThai))
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ cho phép: Chưa thanh toán, Đã thanh toán.");

            var hoaDons = await _context.HoaDons
                .Where(hd => hd.TrangThai == trangThai)
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();

            return hoaDons.Select(hd => MapToVM(hd));
        }
    }
}
