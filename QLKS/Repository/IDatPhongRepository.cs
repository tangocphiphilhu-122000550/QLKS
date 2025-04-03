using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly Qlks1Context _context;

        public DatPhongRepository(Qlks1Context context)
        {
            _context = context;
        }

        public async Task<DatPhong> AddAsync(DatPhong datPhong)
        {
            _context.DatPhongs.Add(datPhong);
            await _context.SaveChangesAsync();
            return datPhong;
        }

        public async Task<DatPhong> UpdateAsync(DatPhong datPhong)
        {
            _context.Entry(datPhong).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return datPhong;
        }

        public async Task<bool> DeleteAsync(int maDatPhong)
        {
            var datPhong = await _context.DatPhongs.FindAsync(maDatPhong);
            if (datPhong == null)
                return false;

            _context.DatPhongs.Remove(datPhong);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DatPhong>> GetAllAsync()
        {
            return await _context.DatPhongs
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.MaNvNavigation)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                .ToListAsync();
        }

        public async Task<DatPhong?> GetByIdAsync(int maDatPhong)
        {
            return await _context.DatPhongs
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.MaNvNavigation)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);
        }

        public async Task<IEnumerable<DatPhong>> GetByMaKhAsync(int maKh)
        {
            return await _context.DatPhongs
                .Where(dp => dp.MaKh == maKh)
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.MaNvNavigation)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                .ToListAsync();
        }

        public async Task<IEnumerable<DatPhong>> GetByMaNvAsync(int maNv)
        {
            return await _context.DatPhongs
                .Where(dp => dp.MaNv == maNv)
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.MaNvNavigation)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                .ToListAsync();
        }

        public async Task<IEnumerable<DatPhong>> GetByMaPhongAsync(string maPhong)
        {
            return await _context.DatPhongs
                .Where(dp => dp.MaPhong == maPhong)
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.MaNvNavigation)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                .ToListAsync();
        }

        public async Task<IEnumerable<DatPhong>> GetByTrangThaiAsync(string trangThai)
        {
            return await _context.DatPhongs
                .Where(dp => dp.TrangThai == trangThai)
                .Include(dp => dp.MaKhNavigation)
                .Include(dp => dp.MaNvNavigation)
                .Include(dp => dp.MaPhongNavigation)
                .Include(dp => dp.SuDungDichVus)
                .ToListAsync();
        }

        public async Task<bool> IsPhongDatAsync(string maPhong, DateOnly ngayNhanPhong, DateOnly ngayTraPhong)
        {
            return await _context.DatPhongs
                .AnyAsync(dp => dp.MaPhong == maPhong &&
                              ((ngayNhanPhong >= dp.NgayNhanPhong && ngayNhanPhong < dp.NgayTraPhong) ||
                               (ngayTraPhong > dp.NgayNhanPhong && ngayTraPhong <= dp.NgayTraPhong) ||
                               (ngayNhanPhong <= dp.NgayNhanPhong && ngayTraPhong >= dp.NgayTraPhong)));
        }
    }
}
