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
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                var existingUser = await _repository.GetNhanVienByEmail(model.Email);
                if (existingUser == null)
                    return BadRequest(new { Message = "Email chưa được thêm vào hệ thống. Vui lòng dùng API AddAccount trước." });

                if (!existingUser.IsActive)
                    return BadRequest(new { Message = "Tài khoản đã bị vô hiệu hóa, không thể đăng ký." });

                if (existingUser.MatKhau != null && existingUser.MatKhau.Length > 0)
                    return BadRequest(new { Message = "Tài khoản đã được đăng ký trước đó." });

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

                var nhanVienToUpdate = new NhanVien
                {
                    Email = model.Email,
                    MatKhau = passwordBytes
                };

                var updatedNhanVien = await _repository.Register(nhanVienToUpdate);
                return Ok(new { Message = "Đăng ký thành công!", MaNv = updatedNhanVien.MaNv });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi đăng ký: " + ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var (nhanVien, token, refreshToken) = await _repository.Login(model.Email, model.MatKhau);
            if (nhanVien == null)
            {
                var existingUser = await _repository.GetNhanVienByEmail(model.Email);
                if (existingUser != null && !existingUser.IsActive)
                {
                    return Unauthorized(new { Message = "Tài khoản đã bị vô hiệu hóa." });
                }
                return Unauthorized(new { Message = "Email hoặc mật khẩu không đúng." });
            }

            var response = new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                HoTen = nhanVien.HoTen,
                Email = nhanVien.Email
            };

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO model)
        {
            try
            {
                var (newToken, newRefreshToken) = await _repository.RefreshToken(model.Token, model.RefreshToken);
                return Ok(new { Token = newToken, RefreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var success = await _repository.RevokeToken(token);
            if (!success)
            {
                return BadRequest(new { Message = "Không thể đăng xuất. Token không hợp lệ." });
            }

            return Ok(new { Message = "Đăng xuất thành công!" });
        }

        [HttpPost("forgot-password")]
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
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var (nhanVien, token, refreshToken) = await _repository.Login(model.Email, model.OldPassword);
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
    }
}