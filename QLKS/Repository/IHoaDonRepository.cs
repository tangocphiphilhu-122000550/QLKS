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
        Task<IEnumerable<HoaDonVM>> GetByTenKhachHangAsync(string tenKhachHang);
        Task<HoaDonVM> CreateAsync(CreateHoaDonVM hoaDonVM);
        Task<bool> UpdateTrangThaiByTenKhachHangAsync(string tenKhachHang, UpdateHoaDonVM updateVM);
        Task<bool> UpdatePhuongThucThanhToanByTenKhachHangAsync(string tenKhachHang, UpdatePhuongThucThanhToanVM updateVM);
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
            Console.WriteLine("Bắt đầu lấy danh sách tất cả hóa đơn...");

            var hoaDons = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .ToListAsync();

            Console.WriteLine($"Tìm thấy {hoaDons.Count} hóa đơn.");
            return hoaDons.Select(hd => MapToVM(hd));
        }

        public async Task<IEnumerable<HoaDonVM>> GetByTenKhachHangAsync(string tenKhachHang)
        {
            Console.WriteLine($"Bắt đầu tìm kiếm hóa đơn theo tên khách hàng: {tenKhachHang}");

            if (string.IsNullOrWhiteSpace(tenKhachHang))
                throw new ArgumentException("Tên khách hàng không được để trống.");

            var hoaDons = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Include(hd => hd.MaNvNavigation)
                .Include(hd => hd.ChiTietHoaDons)
                    .ThenInclude(cthd => cthd.MaDatPhongNavigation)
                        .ThenInclude(dp => dp.SuDungDichVus)
                            .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Where(hd => hd.MaKhNavigation.HoTen.Contains(tenKhachHang))
                .ToListAsync();

            Console.WriteLine($"Tìm thấy {hoaDons.Count} hóa đơn cho tên khách hàng: {tenKhachHang}");
            return hoaDons.Select(hd => MapToVM(hd));
        }

        public async Task<HoaDonVM> CreateAsync(CreateHoaDonVM hoaDonVM)
        {
            Console.WriteLine("Bắt đầu tạo hóa đơn...");

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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
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

                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Đã tạo hóa đơn với MaHoaDon: {hoaDon.MaHoaDon}");

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Lỗi khi tạo hóa đơn: {ex.Message}");
                    throw new Exception($"Lỗi khi tạo hóa đơn: {ex.Message}", ex);
                }
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

        public async Task<bool> UpdateTrangThaiByTenKhachHangAsync(string tenKhachHang, UpdateHoaDonVM updateVM)
        {
            Console.WriteLine($"Bắt đầu cập nhật trạng thái hóa đơn theo tên khách hàng: {tenKhachHang}");

            if (string.IsNullOrWhiteSpace(tenKhachHang))
                throw new ArgumentException("Tên khách hàng không được để trống.");

            if (updateVM == null || string.IsNullOrWhiteSpace(updateVM.TrangThai))
                throw new ArgumentException("Trạng thái không được để trống.");

            var validTrangThai = new[] { "Chưa thanh toán", "Đã thanh toán" };
            if (!validTrangThai.Contains(updateVM.TrangThai))
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ cho phép: Chưa thanh toán, Đã thanh toán.");

            var hoaDons = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Where(hd => hd.MaKhNavigation.HoTen.Contains(tenKhachHang))
                .ToListAsync();

            if (!hoaDons.Any())
            {
                Console.WriteLine($"Không tìm thấy hóa đơn nào cho tên khách hàng: {tenKhachHang}");
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var hoaDon in hoaDons)
                    {
                        hoaDon.TrangThai = updateVM.TrangThai;
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Cập nhật trạng thái thành công cho {hoaDons.Count} hóa đơn của khách hàng: {tenKhachHang}");

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Lỗi khi cập nhật trạng thái: {ex.Message}");
                    throw new Exception($"Lỗi khi cập nhật trạng thái: {ex.Message}", ex);
                }
            }

            return true;
        }

        public async Task<bool> UpdatePhuongThucThanhToanByTenKhachHangAsync(string tenKhachHang, UpdatePhuongThucThanhToanVM updateVM)
        {
            Console.WriteLine($"Bắt đầu cập nhật phương thức thanh toán theo tên khách hàng: {tenKhachHang}");

            if (string.IsNullOrWhiteSpace(tenKhachHang))
                throw new ArgumentException("Tên khách hàng không được để trống.");

            if (updateVM == null || string.IsNullOrWhiteSpace(updateVM.PhuongThucThanhToan))
                throw new ArgumentException("Phương thức thanh toán không được để trống.");

            var validPhuongThucThanhToan = new[] { "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng" };
            if (!validPhuongThucThanhToan.Contains(updateVM.PhuongThucThanhToan, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Phương thức thanh toán không hợp lệ. Chỉ cho phép: Tiền mặt, Chuyển khoản, Thẻ tín dụng.");

            var hoaDons = await _context.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Where(hd => hd.MaKhNavigation.HoTen.Contains(tenKhachHang))
                .ToListAsync();

            if (!hoaDons.Any())
            {
                Console.WriteLine($"Không tìm thấy hóa đơn nào cho tên khách hàng: {tenKhachHang}");
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var hoaDon in hoaDons)
                    {
                        hoaDon.PhuongThucThanhToan = updateVM.PhuongThucThanhToan;
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Cập nhật phương thức thanh toán thành công cho {hoaDons.Count} hóa đơn của khách hàng: {tenKhachHang}");

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Lỗi khi cập nhật phương thức thanh toán: {ex.Message}");
                    throw new Exception($"Lỗi khi cập nhật phương thức thanh toán: {ex.Message}", ex);
                }
            }

            return true;
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

            var validPhuongThucThanhToan = new[] { "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng" };
            if (!string.IsNullOrEmpty(hoaDon.PhuongThucThanhToan) && !validPhuongThucThanhToan.Contains(hoaDon.PhuongThucThanhToan, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Phương thức thanh toán không hợp lệ. Chỉ cho phép: Tiền mặt, Chuyển khoản, Thẻ tín dụng.");
        }

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
                        TongTienDichVu = cthd.MaDatPhongNavigation?.SuDungDichVus?.Sum(sddv => sddv.ThanhTien ?? 0) ?? 0,
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
    }
}