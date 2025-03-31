namespace QLKS.Models
{
    public class LoginDTO
    {
        public string Email { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
    }

    public class RegisterDTO
    {
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string MatKhau { get; set; } = null!; // Không cần MaVaiTro
    }

    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDTO
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ChangePasswordDTO
    {
        public string Email { get; set; } = null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class Account
    {
        public string? HoTen { get; set; } 
        public int? MaVaiTro { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public DateOnly? NgaySinh { get; set; }
    }
}