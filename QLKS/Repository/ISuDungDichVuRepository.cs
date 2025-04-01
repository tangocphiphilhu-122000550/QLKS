using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;
using System; // Để sử dụng DateOnly và TimeOnly

namespace QLKS.Repository
{
    public interface ISuDungDichVuRepository
    {
        Task<List<SuDungDichVuVM>> GetAllSuDungDichVu();
        Task<SuDungDichVuVM> AddSuDungDichVu(SuDungDichVuVM suDungDichVuVM);
        Task<bool> UpdateSuDungDichVu(int maSuDung, SuDungDichVuVM suDungDichVuVM);
        Task<bool> DeleteSuDungDichVu(int maSuDung);
    }

    public class SuDungDichVuRepository : ISuDungDichVuRepository
    {
        private readonly Qlks1Context _context;

        public SuDungDichVuRepository(Qlks1Context context)
        {
            _context = context;
        }

        public async Task<List<SuDungDichVuVM>> GetAllSuDungDichVu()
        {
            return await _context.SuDungDichVus
                .AsNoTracking()
                .Select(sddv => new SuDungDichVuVM
                {
                    MaDatPhong = sddv.MaDatPhong,
                    MaDichVu = sddv.MaDichVu,
                    SoLuong = sddv.SoLuong,
                    NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null, // Chuyển DateOnly sang DateTime
                    NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null, // Chuyển DateOnly sang DateTime
                    ThanhTien = sddv.ThanhTien
                })
                .ToListAsync();
        }

        public async Task<SuDungDichVuVM> AddSuDungDichVu(SuDungDichVuVM suDungDichVuVM)
        {
            if (suDungDichVuVM.MaDatPhong == null || suDungDichVuVM.MaDichVu == null || suDungDichVuVM.SoLuong <= 0)
            {
                throw new ArgumentException("Mã đặt phòng, mã dịch vụ và số lượng không hợp lệ.");
            }

            if (suDungDichVuVM.NgaySuDung == null)
            {
                throw new ArgumentException("Ngày sử dụng không được để trống.");
            }

            // Kiểm tra logic: Ngày kết thúc phải lớn hơn hoặc bằng ngày sử dụng (nếu có)
            if (suDungDichVuVM.NgayKetThuc.HasValue && suDungDichVuVM.NgayKetThuc < suDungDichVuVM.NgaySuDung)
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn hoặc bằng ngày sử dụng.");
            }

            var datPhong = await _context.DatPhongs.FindAsync(suDungDichVuVM.MaDatPhong);
            if (datPhong == null)
            {
                throw new ArgumentException("Mã đặt phòng không tồn tại.");
            }

            var dichVu = await _context.DichVus.FindAsync(suDungDichVuVM.MaDichVu);
            if (dichVu == null)
            {
                throw new ArgumentException("Mã dịch vụ không tồn tại.");
            }

            decimal thanhTien = suDungDichVuVM.ThanhTien ?? (dichVu.DonGia * suDungDichVuVM.SoLuong);

            var suDungDichVu = new QLKS.Data.SuDungDichVu
            {
                MaDatPhong = suDungDichVuVM.MaDatPhong,
                MaDichVu = suDungDichVuVM.MaDichVu,
                SoLuong = suDungDichVuVM.SoLuong,
                NgaySuDung = DateOnly.FromDateTime(suDungDichVuVM.NgaySuDung.Value), // Chuyển DateTime sang DateOnly
                NgayKetThuc = suDungDichVuVM.NgayKetThuc.HasValue ? DateOnly.FromDateTime(suDungDichVuVM.NgayKetThuc.Value) : (DateOnly?)null, // Chuyển DateTime sang DateOnly
                ThanhTien = thanhTien
            };

            _context.SuDungDichVus.Add(suDungDichVu);
            await _context.SaveChangesAsync();

            return new SuDungDichVuVM
            {
                MaDatPhong = suDungDichVu.MaDatPhong,
                MaDichVu = suDungDichVu.MaDichVu,
                SoLuong = suDungDichVu.SoLuong,
                NgaySuDung = suDungDichVu.NgaySuDung.HasValue ? suDungDichVu.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null, // Chuyển DateOnly sang DateTime
                NgayKetThuc = suDungDichVu.NgayKetThuc.HasValue ? suDungDichVu.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null, // Chuyển DateOnly sang DateTime
                ThanhTien = suDungDichVu.ThanhTien
            };
        }


        public async Task<bool> UpdateSuDungDichVu(int maSuDung, SuDungDichVuVM suDungDichVuVM)
        {
            if (suDungDichVuVM.MaDatPhong == null || suDungDichVuVM.MaDichVu == null || suDungDichVuVM.SoLuong <= 0)
            {
                throw new ArgumentException("Mã đặt phòng, mã dịch vụ và số lượng không hợp lệ.");
            }

            if (suDungDichVuVM.NgaySuDung == null)
            {
                throw new ArgumentException("Ngày sử dụng không được để trống.");
            }

            // Kiểm tra logic: Ngày kết thúc phải lớn hơn hoặc bằng ngày sử dụng (nếu có)
            if (suDungDichVuVM.NgayKetThuc.HasValue && suDungDichVuVM.NgayKetThuc < suDungDichVuVM.NgaySuDung)
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn hoặc bằng ngày sử dụng.");
            }

            var existingSuDungDichVu = await _context.SuDungDichVus
                .FirstOrDefaultAsync(sddv => sddv.MaSuDung == maSuDung);
            if (existingSuDungDichVu == null)
            {
                return false;
            }

            var datPhong = await _context.DatPhongs.FindAsync(suDungDichVuVM.MaDatPhong);
            if (datPhong == null)
            {
                throw new ArgumentException("Mã đặt phòng không tồn tại.");
            }

            var dichVu = await _context.DichVus.FindAsync(suDungDichVuVM.MaDichVu);
            if (dichVu == null)
            {
                throw new ArgumentException("Mã dịch vụ không tồn tại.");
            }

            decimal thanhTien = suDungDichVuVM.ThanhTien ?? (dichVu.DonGia * suDungDichVuVM.SoLuong);

            existingSuDungDichVu.MaDatPhong = suDungDichVuVM.MaDatPhong;
            existingSuDungDichVu.MaDichVu = suDungDichVuVM.MaDichVu;
            existingSuDungDichVu.SoLuong = suDungDichVuVM.SoLuong;
            existingSuDungDichVu.NgaySuDung = DateOnly.FromDateTime(suDungDichVuVM.NgaySuDung.Value); 
            existingSuDungDichVu.NgayKetThuc = suDungDichVuVM.NgayKetThuc.HasValue ? DateOnly.FromDateTime(suDungDichVuVM.NgayKetThuc.Value) : null; 
            existingSuDungDichVu.ThanhTien = thanhTien;

            _context.SuDungDichVus.Update(existingSuDungDichVu);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSuDungDichVu(int maSuDung)
        {
            var suDungDichVu = await _context.SuDungDichVus
                .FirstOrDefaultAsync(sddv => sddv.MaSuDung == maSuDung);
            if (suDungDichVu == null)
            {
                return false;
            }

            _context.SuDungDichVus.Remove(suDungDichVu);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}