// IDatPhongRepository.cs
using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


namespace QLKS.Repository
{
    public interface IDatPhongRepository
    {
        Task<IEnumerable<DatPhongVM>> GetAllVMAsync();
        Task<IEnumerable<DatPhongVM>> GetByMaPhongVMAsync(string maPhong);
        Task<IEnumerable<DatPhongVM>> GetByTenKhachHangVMAsync(string tenKhachHang);
        Task<IEnumerable<DatPhongVM>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<DatPhongVM> AddVMAsync(CreateDatPhongVM datPhongVM);
        Task<DatPhongVM> UpdateVMAsync(int maDatPhong, UpdateDatPhongVM datPhongVM);
        Task<bool> DeleteByMaDatPhongAsync(int maDatPhong);
        Task UpdateDatPhongStatusAsync();
        Task CancelDatPhongAsync(int maDatPhong);
    }
    public class DatPhongRepository : IDatPhongRepository
    {
        private readonly DataQlks112Nhom3Context _context;
        private readonly IHoaDonRepository _hoaDonRepository;

        public DatPhongRepository(DataQlks112Nhom3Context context, IHoaDonRepository hoaDonRepository)
        {
            _context = context;
            _hoaDonRepository = hoaDonRepository;
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

        public async Task<IEnumerable<DatPhongVM>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");

            var datPhongs = await _context.DatPhongs
                .Where(dp => dp.NgayNhanPhong >= startDate && dp.NgayTraPhong <= endDate)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                    .ThenInclude(sddv => sddv.MaDichVuNavigation)
                .Include(dp => dp.MaKhNavigation)
                .ToListAsync();

            return datPhongs.Select(dp => MapToVM(dp));
        }

        public async Task<DatPhongVM> AddVMAsync(CreateDatPhongVM datPhongVM)
        {
            if (datPhongVM == null)
                throw new ArgumentNullException(nameof(datPhongVM));

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
                TrangThai = "Đang sử dụng"
            };

            datPhong.PhuThu = await CalculatePhuThu(datPhong.MaPhong, datPhong.SoNguoiO, datPhong.NgayNhanPhong, datPhong.NgayTraPhong);
            datPhong.TongTienPhong = await CalculateTongTienPhong(datPhong.MaPhong, datPhong.NgayNhanPhong, datPhong.NgayTraPhong, datPhong.PhuThu);

            await ValidateDatPhong(datPhong);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.DatPhongs.Add(datPhong);
                await _context.SaveChangesAsync();

                if (datPhongVM.DanhSachDichVu != null && datPhongVM.DanhSachDichVu.Any())
                {
                    foreach (var dv in datPhongVM.DanhSachDichVu)
                    {
                        if (!await _context.DichVus.AnyAsync(d => d.MaDichVu == dv.MaDichVu))
                            throw new ArgumentException($"Dịch vụ với MaDichVu {dv.MaDichVu} không tồn tại.");
                        if (dv.SoLuong <= 0)
                            throw new ArgumentException($"Số lượng dịch vụ {dv.MaDichVu} phải lớn hơn 0.");
                        if (dv.ThanhTien < 0)
                            throw new ArgumentException($"Thành tiền của dịch vụ {dv.MaDichVu} không được âm.");
                        if (dv.NgaySuDung.HasValue && dv.NgayKetThuc.HasValue && dv.NgaySuDung > dv.NgayKetThuc)
                            throw new ArgumentException($"Ngày sử dụng của dịch vụ {dv.MaDichVu} không được sau ngày kết thúc.");

                        var suDungDichVu = new QLKS.Data.SuDungDichVu
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
                    await _context.SaveChangesAsync();
                }

                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                if (currentDate > datPhong.NgayTraPhong && datPhong.TrangThai != "Hoàn thành")
                {
                    datPhong.TrangThai = "Hoàn thành";
                    await _context.SaveChangesAsync();
                    await CreateHoaDonIfNeeded(datPhong);
                }

                await UpdatePhongTrangThai(datPhong.MaPhong, datPhong.TrangThai);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi thêm đặt phòng: {ex.Message}", ex);
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
                TrangThai = datPhongVM.TrangThai?.Trim()
            };

            datPhong.PhuThu = await CalculatePhuThu(datPhong.MaPhong, datPhong.SoNguoiO, datPhong.NgayNhanPhong, datPhong.NgayTraPhong);
            datPhong.TongTienPhong = await CalculateTongTienPhong(datPhong.MaPhong, datPhong.NgayNhanPhong, datPhong.NgayTraPhong, datPhong.PhuThu);

            await ValidateDatPhong(datPhong);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Entry(existingDatPhong).CurrentValues.SetValues(datPhong);
                await _context.SaveChangesAsync();

                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                if (currentDate > datPhong.NgayTraPhong && datPhong.TrangThai != "Hoàn thành")
                {
                    datPhong.TrangThai = "Hoàn thành";
                    await _context.SaveChangesAsync();
                    await CreateHoaDonIfNeeded(datPhong);
                }

                await UpdatePhongTrangThai(datPhong.MaPhong, datPhong.TrangThai);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi cập nhật đặt phòng: {ex.Message}", ex);
            }

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

            var chiTietHoaDon = await _context.ChiTietHoaDons
                .Include(cthd => cthd.MaHoaDonNavigation)
                .FirstOrDefaultAsync(cthd => cthd.MaDatPhong == maDatPhong);
            if (chiTietHoaDon != null && chiTietHoaDon.MaHoaDonNavigation?.TrangThai == "Chưa thanh toán")
                throw new InvalidOperationException("Không thể xóa đặt phòng vì có hóa đơn chưa thanh toán liên quan.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var suDungDichVus = await _context.SuDungDichVus
                    .Where(sddv => sddv.MaDatPhong == maDatPhong)
                    .ToListAsync();
                if (suDungDichVus.Any())
                {
                    _context.SuDungDichVus.RemoveRange(suDungDichVus);
                }

                if (chiTietHoaDon != null)
                {
                    _context.ChiTietHoaDons.Remove(chiTietHoaDon);
                }

                string maPhong = datPhong.MaPhong;
                _context.DatPhongs.Remove(datPhong);
                await _context.SaveChangesAsync();

                await UpdatePhongTrangThai(maPhong, "Trống");
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi xóa đặt phòng: {ex.Message}", ex);
            }

            return true;
        }

        public async Task UpdateDatPhongStatusAsync()
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var datPhongs = await _context.DatPhongs
                .Where(dp => dp.TrangThai != "Hủy" && dp.TrangThai != "Hoàn thành")
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.SuDungDichVus)
                .ToListAsync();

            var datPhongsToUpdate = new List<DatPhong>();
            var datPhongsToCreateHoaDon = new List<DatPhong>();

            foreach (var datPhong in datPhongs)
            {
                if (currentDate > datPhong.NgayTraPhong && datPhong.TrangThai != "Hoàn thành")
                {
                    datPhong.TrangThai = "Hoàn thành";
                    datPhongsToUpdate.Add(datPhong);
                    datPhongsToCreateHoaDon.Add(datPhong);
                }
                else if (datPhong.TrangThai != "Đang sử dụng")
                {
                    datPhong.TrangThai = "Đang sử dụng";
                    datPhongsToUpdate.Add(datPhong);
                }
            }

            if (datPhongsToUpdate.Any())
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new Exception($"Lỗi khi cập nhật trạng thái đặt phòng: {ex.InnerException?.Message}", ex);
                }
            }

            foreach (var datPhong in datPhongsToCreateHoaDon)
            {
                await CreateHoaDonIfNeeded(datPhong);
            }

            var maPhongs = datPhongsToUpdate.Select(dp => dp.MaPhong).Distinct().ToList();
            foreach (var maPhong in maPhongs)
            {
                var activeBookings = await _context.DatPhongs
                    .AnyAsync(dp => dp.MaPhong == maPhong && dp.TrangThai == "Đang sử dụng");
                await UpdatePhongTrangThai(maPhong, activeBookings ? "Đang sử dụng" : "Hoàn thành");
            }
        }

        public async Task CancelDatPhongAsync(int maDatPhong)
        {
            var datPhong = await _context.DatPhongs
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);

            if (datPhong == null)
                throw new ArgumentException("Đặt phòng không tồn tại.");

            if (datPhong.TrangThai == "Hủy")
                throw new InvalidOperationException("Đặt phòng đã được hủy trước đó.");

            if (datPhong.TrangThai == "Hoàn thành")
                throw new InvalidOperationException("Không thể hủy đặt phòng đã hoàn thành.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                datPhong.TrangThai = "Hủy";
                await _context.SaveChangesAsync();

                await UpdatePhongTrangThai(datPhong.MaPhong, "Hủy");
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi hủy đặt phòng: {ex.Message}", ex);
            }
        }

        private async Task CreateHoaDonIfNeeded(DatPhong datPhong)
        {
            if (datPhong.TrangThai != "Hoàn thành")
                return;

            var existingChiTiet = await _context.ChiTietHoaDons
                .AnyAsync(cthd => cthd.MaDatPhong == datPhong.MaDatPhong);
            if (existingChiTiet)
            {
                Console.WriteLine($"Đặt phòng {datPhong.MaDatPhong} đã có hóa đơn, bỏ qua.");
                return;
            }

            var createHoaDonVM = new CreateHoaDonVM
            {
                MaKh = datPhong.MaKh,
                MaNv = datPhong.MaNv,
                NgayLap = DateOnly.FromDateTime(DateTime.Now),
                PhuongThucThanhToan = "Chưa xác định",
                TrangThai = "Chưa thanh toán",
                MaDatPhongs = new List<int> { datPhong.MaDatPhong }
            };

            try
            {
                var hoaDonVM = await _hoaDonRepository.AddAsync(createHoaDonVM);
                Console.WriteLine($"Tạo hóa đơn thành công cho DatPhong {datPhong.MaDatPhong}, MaHoaDon: {hoaDonVM.MaHoaDon}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo hóa đơn cho DatPhong {datPhong.MaDatPhong}: {ex.Message}");
                throw;
            }
        }

        private async Task ValidateDatPhong(DatPhong datPhong)
        {
            if (string.IsNullOrEmpty(datPhong.MaPhong))
                throw new ArgumentException("Mã phòng không được để trống.");

            if (datPhong.NgayNhanPhong == default || datPhong.NgayTraPhong == default)
                throw new ArgumentException("Ngày nhận phòng và ngày trả phòng không được để trống.");

            if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
                throw new ArgumentException("Ngày nhận phòng phải trước ngày trả phòng.");

            if (datPhong.SoNguoiO <= 0)
                throw new ArgumentException("Số người ở phải lớn hơn 0.");

            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == datPhong.MaPhong);
            if (phong == null)
                throw new ArgumentException("Phòng không tồn tại.");

            if (phong.TrangThai == "Bảo trì" && datPhong.TrangThai != "Hủy")
                throw new ArgumentException("Phòng đang bảo trì, không thể đặt hoặc sử dụng.");

            if (phong.MaLoaiPhongNavigation != null && datPhong.SoNguoiO > phong.MaLoaiPhongNavigation.SoNguoiToiDa * 2)
                throw new ArgumentException($"Số người ở vượt quá giới hạn tối đa của loại phòng ({phong.MaLoaiPhongNavigation.SoNguoiToiDa * 2} người).");

            if (datPhong.TrangThai != "Hủy")
            {
                var overlappingBookings = await _context.DatPhongs
                    .AnyAsync(dp => dp.MaPhong == datPhong.MaPhong
                        && dp.MaDatPhong != datPhong.MaDatPhong
                        && dp.TrangThai != "Hủy"
                        && dp.NgayNhanPhong <= datPhong.NgayTraPhong
                        && dp.NgayTraPhong >= datPhong.NgayNhanPhong);
                if (overlappingBookings)
                    throw new ArgumentException("Phòng đã được đặt trong khoảng thời gian này.");
            }

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
            {
                Console.WriteLine($"Không tìm thấy phòng {maPhong} hoặc loại phòng.");
                return 0;
            }

            int soNgayO = ngayTraPhong.DayNumber - ngayNhanPhong.DayNumber;
            if (soNgayO <= 0)
            {
                Console.WriteLine($"Số ngày ở không hợp lệ: {soNgayO}. Trả về 0.");
                return 0;
            }

            var phuThu = await _context.PhuThus
                .FirstOrDefaultAsync(pt => pt.MaLoaiPhong == phong.MaLoaiPhong);
            if (phuThu == null || phuThu.PhuThuNguoiThem == null)
            {
                Console.WriteLine($"Không tìm thấy phụ thu cho loại phòng {phong.MaLoaiPhong}.");
                return 0;
            }

            var loaiPhong = phong.MaLoaiPhongNavigation;
            int soNguoiToiDa = loaiPhong?.SoNguoiToiDa ?? 2;
            int soNguoiThem = Math.Max(0, soNguoiO - soNguoiToiDa);

            // Sử dụng toán tử ?? để gán giá trị mặc định 0 nếu PhuThuNguoiThem là null
            decimal phuThuNguoiThem = phuThu.PhuThuNguoiThem ?? 0;
            decimal result = soNguoiThem * phuThuNguoiThem * soNgayO;

            Console.WriteLine($"Tính phụ thu: MaPhong={maPhong}, SoNguoiO={soNguoiO}, SoNguoiToiDa={soNguoiToiDa}, SoNguoiThem={soNguoiThem}, SoNgayO={soNgayO}, PhuThuNguoiThem={phuThuNguoiThem}, Result={result}");
            return result;
        }

        private async Task<decimal> CalculateTongTienPhong(string maPhong, DateOnly ngayNhanPhong, DateOnly ngayTraPhong, decimal? phuThu)
        {
            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null || phong.MaLoaiPhongNavigation == null)
            {
                Console.WriteLine($"Không tìm thấy phòng {maPhong} hoặc loại phòng.");
                return 0;
            }

            int soNgayO = ngayTraPhong.DayNumber - ngayNhanPhong.DayNumber;
            if (soNgayO <= 0)
            {
                Console.WriteLine($"Số ngày ở không hợp lệ: {soNgayO}. Trả về 0.");
                return 0;
            }

            decimal giaPhong = phong.MaLoaiPhongNavigation.GiaCoBan;
            decimal result = (giaPhong * soNgayO) + (phuThu ?? 0);

            Console.WriteLine($"Tính tổng tiền phòng: MaPhong={maPhong}, GiaPhong={giaPhong}, SoNgayO={soNgayO}, PhuThu={phuThu}, Result={result}");
            return result;
        }

        private async Task UpdatePhongTrangThai(string maPhong, string datPhongTrangThai)
        {
            var phong = await _context.Phongs.FirstOrDefaultAsync(p => p.MaPhong == maPhong);
            if (phong == null)
                throw new Exception($"Phòng {maPhong} không tồn tại.");

            if (phong.TrangThai == "Bảo trì")
                return;

            var activeBookings = await _context.DatPhongs
                .AnyAsync(dp => dp.MaPhong == maPhong && dp.TrangThai == "Đang sử dụng");

            if (datPhongTrangThai == "Đang sử dụng")
            {
                phong.TrangThai = "Đang sử dụng";
            }
            else if (datPhongTrangThai == "Hủy" || datPhongTrangThai == "Hoàn thành" || datPhongTrangThai == "Trống")
            {
                phong.TrangThai = activeBookings ? "Đang sử dụng" : "Trống";
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Cập nhật trạng thái phòng: MaPhong={maPhong}, TrangThai={phong.TrangThai}");
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
                    MaDatPhong = sddv.MaDatPhong,
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
