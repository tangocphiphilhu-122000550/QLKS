using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLKS.Repository
{
    public interface INhanVienRepository
    {
        Task<NhanVien> Register(NhanVien nhanVien);
        Task<NhanVien> Login(string email, string matKhau);
        Task<NhanVien> GetNhanVienByEmail(string email);
        Task<bool> UpdatePassword(string email, byte[] newPassword);
        Task<bool> ForgotPassword(string email);
    }

    public class NhanVienRepository : INhanVienRepository
    {
        private readonly Qlks1Context _context;
        private readonly EmailHelper _emailHelper;

        public NhanVienRepository(Qlks1Context context, EmailHelper emailHelper)
        {
            _context = context;
            _emailHelper = emailHelper;
        }

        public async Task<NhanVien> Register(NhanVien nhanVien)
        {
            var existingUser = await GetNhanVienByEmail(nhanVien.Email);
            if (existingUser != null)
                throw new Exception("Email đã được sử dụng.");

            try
            {
                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();
                return nhanVien;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public async Task<NhanVien> Login(string email, string matKhau)
        {
            var nhanVien = await _context.NhanViens
                .Include(nv => nv.MaVaiTroNavigation)
                .FirstOrDefaultAsync(nv => nv.Email == email);

            if (nhanVien == null || !BCrypt.Net.BCrypt.Verify(matKhau, Encoding.UTF8.GetString(nhanVien.MatKhau)))
            {
                return null;
            }

            return nhanVien;
        }

        public async Task<NhanVien> GetNhanVienByEmail(string email)
        {
            return await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.Email == email);
        }

        public async Task<bool> UpdatePassword(string email, byte[] newPassword)
        {
            var nhanVien = await GetNhanVienByEmail(email);
            if (nhanVien == null)
            {
                return false;
            }

            nhanVien.MatKhau = newPassword;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPassword(string email)
        {
            var nhanVien = await GetNhanVienByEmail(email);
            if (nhanVien == null)
            {
                return false;
            }

            string newPassword = GenerateRandomPassword(8);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

            var success = await UpdatePassword(email, passwordBytes);
            if (!success)
            {
                return false;
            }

            string subject = "Yêu Cầu Đặt Lại Mật Khẩu - Khách Sạn Hoàng Gia";
            string body = $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <h3>Xin chào {nhanVien.HoTen},</h3>
                    <p>Mật khẩu mới của bạn là: <strong>{newPassword}</strong></p>
                    <p>Vui lòng đổi mật khẩu sau khi đăng nhập.</p>
                </body>
                </html>";

            await _emailHelper.SendEmailAsync(email, subject, body, isHtml: true);
            return true;
        }

        private string GenerateRandomPassword(int length)
        {
            const string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random random = new Random();
            return new string(Enumerable.Repeat(allChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}