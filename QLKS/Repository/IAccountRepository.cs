using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLKS.Repository
{
    public interface IAccountRepository
    {
        Task<List<NhanVien>> GetAllAccounts();
        Task<List<NhanVien>> GetByNameNhanVien(string hoTen);
        Task<NhanVien> AddAccount(NhanVien nhanVien);
        Task<bool> UpdateAccount(string email, NhanVien nhanVien);
        Task<bool> DeleteAccount(string email); // Giữ tên nhưng đổi thành vô hiệu hóa
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly Qlks1Context _context;

        public AccountRepository(Qlks1Context context)
        {
            _context = context;
        }

        public async Task<List<NhanVien>> GetAllAccounts()
        {
            return await _context.NhanViens
                .Include(nv => nv.MaVaiTroNavigation)
                .Where(nv => nv.IsActive) // Chỉ lấy nhân viên đang hoạt động
                .ToListAsync();
        }

        public async Task<List<NhanVien>> GetByNameNhanVien(string hoTen)
        {
            return await _context.NhanViens
                .Include(nv => nv.MaVaiTroNavigation)
                .Where(nv => nv.HoTen.Contains(hoTen) && nv.IsActive) // Chỉ lấy nhân viên đang hoạt động
                .ToListAsync();
        }

        public async Task<NhanVien> AddAccount(NhanVien nhanVien)
        {
            var existingUser = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.Email == nhanVien.Email);
            if (existingUser != null)
                throw new Exception("Email đã được sử dụng.");

            var vaiTro = await _context.VaiTros
                .FirstOrDefaultAsync(vt => vt.MaVaiTro == nhanVien.MaVaiTro);
            if (vaiTro == null)
                throw new Exception("Mã vai trò không tồn tại.");

            nhanVien.IsActive = true; // Đảm bảo nhân viên mới là active
            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();
            return nhanVien;
        }

        public async Task<bool> UpdateAccount(string email, NhanVien nhanVien)
        {
            var existingNhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.Email == email);
            if (existingNhanVien == null)
                return false;

            if (nhanVien.Email != null && nhanVien.Email != email)
            {
                var emailDuplicate = await _context.NhanViens
                    .FirstOrDefaultAsync(nv => nv.Email == nhanVien.Email);
                if (emailDuplicate != null)
                    throw new Exception("Email đã được sử dụng bởi tài khoản khác.");
                existingNhanVien.Email = nhanVien.Email;
            }

            if (nhanVien.HoTen != null) existingNhanVien.HoTen = nhanVien.HoTen;
            if (nhanVien.SoDienThoai != null) existingNhanVien.SoDienThoai = nhanVien.SoDienThoai;
            if (nhanVien.MaVaiTro.HasValue)
            {
                var vaiTro = await _context.VaiTros
                    .FirstOrDefaultAsync(vt => vt.MaVaiTro == nhanVien.MaVaiTro.Value);
                if (vaiTro == null)
                    throw new Exception("Mã vai trò không tồn tại.");
                existingNhanVien.MaVaiTro = nhanVien.MaVaiTro;
            }
            if (nhanVien.GioiTinh != null) existingNhanVien.GioiTinh = nhanVien.GioiTinh;
            if (nhanVien.DiaChi != null) existingNhanVien.DiaChi = nhanVien.DiaChi;
            if (nhanVien.NgaySinh.HasValue) existingNhanVien.NgaySinh = nhanVien.NgaySinh;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccount(string email)
        {
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.Email == email);
            if (nhanVien == null)
                return false;

            if (!nhanVien.IsActive)
                return false; // Đã vô hiệu hóa trước đó

            nhanVien.IsActive = false; // Vô hiệu hóa nhân viên
            await _context.SaveChangesAsync();
            return true;
        }
    }
}