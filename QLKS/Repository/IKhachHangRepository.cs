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
                .Select(kh => new KhachHangMD
                {
                    MaKh = kh.MaKh,
                    HoTen = kh.HoTen,
                    CccdPassport = kh.CccdPassport,
                    SoDienThoai = kh.SoDienThoai,
                    QuocTich = kh.QuocTich,
                    GhiChu = kh.GhiChu
                })
                .ToListAsync();
        }

        public async Task<List<KhachHangMD>> GetKhachHangByName(string hoTen)
        {
            return await _context.KhachHangs
                .AsNoTracking()
                .Where(kh => kh.HoTen.Contains(hoTen))
                .Select(kh => new KhachHangMD
                {
                    MaKh = kh.MaKh,
                    HoTen = kh.HoTen,
                    CccdPassport = kh.CccdPassport,
                    SoDienThoai = kh.SoDienThoai,
                    QuocTich = kh.QuocTich,
                    GhiChu = kh.GhiChu
                })
                .ToListAsync();
        }

        public async Task<KhachHangVM> AddKhachHang(KhachHangVM khachHangVM)
        {
            if (string.IsNullOrEmpty(khachHangVM.HoTen))
            {
                throw new ArgumentException("Họ tên khách hàng không hợp lệ.");
            }

            var khachHang = new KhachHang
            {
                HoTen = khachHangVM.HoTen,
                CccdPassport = khachHangVM.CccdPassport,
                SoDienThoai = khachHangVM.SoDienThoai,
                QuocTich = khachHangVM.QuocTich,
                GhiChu = khachHangVM.GhiChu
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return new KhachHangVM
            {
                HoTen = khachHang.HoTen,
                CccdPassport = khachHang.CccdPassport,
                SoDienThoai = khachHang.SoDienThoai,
                QuocTich = khachHang.QuocTich,
                GhiChu = khachHang.GhiChu
            };
        }

        public async Task<bool> UpdateKhachHang(string hoTen, KhachHangVM khachHangVM)
        {
            if (string.IsNullOrEmpty(khachHangVM.HoTen))
            {
                throw new ArgumentException("Họ tên khách hàng không hợp lệ.");
            }

            var existingKhachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.HoTen == hoTen);
            if (existingKhachHang == null)
            {
                return false;
            }

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
                .FirstOrDefaultAsync(kh => kh.HoTen == hoTen);
            if (khachHang == null)
            {
                return false;
            }

            _context.KhachHangs.Remove(khachHang);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
