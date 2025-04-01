using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;
using QLKS.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AuthController : ControllerBase
    {
        private readonly INhanVienRepository _repository;
        private readonly IConfiguration _configuration;

        public AuthController(INhanVienRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        //[Authorize(Roles = "Quan ly")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                var existingUser = await _repository.GetNhanVienByEmail(model.Email);
                if (existingUser == null)
                {
                    return BadRequest(new { Message = "Email chưa được thêm vào hệ thống. Vui lòng dùng API AddAccount trước." });
                }

                if (!existingUser.IsActive)
                {
                    return BadRequest(new { Message = "Tài khoản đã bị vô hiệu hóa, không thể đăng ký." });
                }

                if (existingUser.MatKhau != null && existingUser.MatKhau.Length > 0)
                {
                    return BadRequest(new { Message = "Tài khoản đã được đăng ký trước đó." });
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

                existingUser.MatKhau = passwordBytes;

                await _repository.Register(existingUser);
                return Ok(new { Message = "Đăng ký thành công!", MaNv = existingUser.MaNv });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi đăng ký: " + ex.Message });
            }
        }

        [HttpPost("login")]
        //[AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var nhanVien = await _repository.Login(model.Email, model.MatKhau);
            if (nhanVien == null)
            {
                var existingUser = await _repository.GetNhanVienByEmail(model.Email);
                if (existingUser != null && !existingUser.IsActive)
                {
                    return Unauthorized(new { Message = "Tài khoản đã bị vô hiệu hóa." });
                }
                return Unauthorized(new { Message = "Email hoặc mật khẩu không đúng." });
            }

            var token = GenerateJwtToken(nhanVien);
            var response = new AuthResponse
            {
                Token = token,
                HoTen = nhanVien.HoTen,
                Email = nhanVien.Email
            };

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        //[Authorize(Roles = "Quan ly")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var success = await _repository.ForgotPassword(model.Email);
            if (!success)
            {
                var existingUser = await _repository.GetNhanVienByEmail(model.Email);
                if (existingUser != null && !existingUser.IsActive)
                {
                    return BadRequest(new { Message = "Tài khoản đã bị vô hiệu hóa, không thể đặt lại mật khẩu." });
                }
                return BadRequest(new { Message = "Email không tồn tại hoặc không thể tạo mật khẩu mới." });
            }

            return Ok(new { Message = "Mật khẩu mới đã được gửi qua email." });
        }

        [HttpPost("change-password")]
         //[Authorize(Roles = "Quan ly")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var nhanVien = await _repository.Login(model.Email, model.OldPassword);
            if (nhanVien == null)
            {
                var existingUser = await _repository.GetNhanVienByEmail(model.Email);
                if (existingUser != null && !existingUser.IsActive)
                {
                    return Unauthorized(new { Message = "Tài khoản đã bị vô hiệu hóa." });
                }
                return Unauthorized(new { Message = "Email hoặc mật khẩu cũ không đúng." });
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

            var success = await _repository.UpdatePassword(model.Email, passwordBytes);
            if (!success)
            {
                return BadRequest(new { Message = "Không thể đổi mật khẩu." });
            }

            return Ok(new { Message = "Đổi mật khẩu thành công!" });
        }

        private string GenerateJwtToken(NhanVien nhanVien)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, nhanVien.MaNv.ToString()),
        new Claim(ClaimTypes.Email, nhanVien.Email),
        new Claim(ClaimTypes.Role, nhanVien.MaVaiTroNavigation?.TenVaiTro ?? "NhanVien") // Role là "Nhân viên" hoặc "Quản lý"
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}