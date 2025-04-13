using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;

namespace QLKS.Repository
{
    public interface IKhachHangRepository
    {
        Task<List<KhachHangMD>> GetAllKhachHang();
        Task<List<KhachHangMD>> GetKhachHangByName(string hoTen);
        Task<KhachHangVM> AddKhachHang(KhachHangVM khachHang);
        Task<bool> UpdateKhachHang(string hoTen, KhachHangVM khachHangVM);
        Task<bool> DeleteKhachHang(string hoTen);
    }

    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public KhachHangRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public async Task<List<KhachHangMD>> GetAllKhachHang()
        {
            return await _context.KhachHangs
                .AsNoTracking()
                .Where(kh => kh.IsActive == true)
                .Select(kh => new KhachHangMD
                {
                    MaKh = kh.MaKh,
                    HoTen = kh.HoTen,
                    CccdPassport = kh.CccdPassport,
                    SoDienThoai = kh.SoDienThoai,
                    QuocTich = kh.QuocTich,
                    GhiChu = kh.GhiChu,
                    MaDatPhong = kh.MaDatPhong
                })
                .ToListAsync();
        }

        public async Task<List<KhachHangMD>> GetKhachHangByName(string hoTen)
        {
            return await _context.KhachHangs
                .AsNoTracking()
                .Where(kh => kh.HoTen.Contains(hoTen) && kh.IsActive == true)
                .Select(kh => new KhachHangMD
                {
                    MaKh = kh.MaKh,
                    HoTen = kh.HoTen,
                    CccdPassport = kh.CccdPassport,
                    SoDienThoai = kh.SoDienThoai,
                    QuocTich = kh.QuocTich,
                    GhiChu = kh.GhiChu,
                    MaDatPhong = kh.MaDatPhong
                })
                .ToListAsync();
        }

        public async Task<KhachHangVM> AddKhachHang(KhachHangVM khachHangVM)
        {
            if (string.IsNullOrEmpty(khachHangVM.HoTen))
            {
                throw new ArgumentException("Họ tên khách hàng không hợp lệ.");
            }

            // THAY ĐỔI: Kiểm tra xem MaDatPhong có tồn tại không nếu được cung cấp
            if (khachHangVM.MaDatPhong.HasValue)
            {
                var datPhong = await _context.DatPhongs
                    .FirstOrDefaultAsync(dp => dp.MaDatPhong == khachHangVM.MaDatPhong && dp.IsActive == true);
                if (datPhong == null)
                {
                    throw new ArgumentException($"Mã đặt phòng {khachHangVM.MaDatPhong} không tồn tại hoặc đã bị ẩn.");
                }
            }

            var khachHang = new KhachHang
            {
                HoTen = khachHangVM.HoTen,
                CccdPassport = khachHangVM.CccdPassport,
                SoDienThoai = khachHangVM.SoDienThoai,
                QuocTich = khachHangVM.QuocTich,
                GhiChu = khachHangVM.GhiChu,
                MaDatPhong = khachHangVM.MaDatPhong, // THAY ĐỔI: Gán MaDatPhong từ khachHangVM
                IsActive = true
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return new KhachHangVM
            {
                HoTen = khachHang.HoTen,
                CccdPassport = khachHang.CccdPassport,
                SoDienThoai = khachHang.SoDienThoai,
                QuocTich = khachHang.QuocTich,
                GhiChu = khachHang.GhiChu,
                MaDatPhong = khachHang.MaDatPhong // THAY ĐỔI: Trả về MaDatPhong trong kết quả
            };
        }

        public async Task<bool> UpdateKhachHang(string hoTen, KhachHangVM khachHangVM)
        {
            if (string.IsNullOrEmpty(khachHangVM.HoTen))
            {
                throw new ArgumentException("Họ tên khách hàng không hợp lệ.");
            }

            var existingKhachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.HoTen == hoTen && kh.IsActive == true);
            if (existingKhachHang == null)
            {
                return false;
            }
            existingKhachHang.MaDatPhong = khachHangVM.MaDatPhong;
            existingKhachHang.HoTen = khachHangVM.HoTen;
            existingKhachHang.CccdPassport = khachHangVM.CccdPassport;
            existingKhachHang.SoDienThoai = khachHangVM.SoDienThoai;
            existingKhachHang.QuocTich = khachHangVM.QuocTich;
            existingKhachHang.GhiChu = khachHangVM.GhiChu;

            _context.KhachHangs.Update(existingKhachHang);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteKhachHang(string hoTen)
        {
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.HoTen == hoTen && kh.IsActive == true);
            if (khachHang == null)
            {
                return false;
            }

            khachHang.IsActive = false;
            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}