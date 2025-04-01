using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;
using QLKS.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _repository;

        public AccountController(IAccountRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("get-all")]
        [Authorize(Roles = "Quản lý")]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var accounts = await _repository.GetAllAccounts();
                var result = accounts.Select(nv => new Account
                {
                    HoTen = nv.HoTen,
                    MaVaiTro = nv.MaVaiTro,
                    SoDienThoai = nv.SoDienThoai,
                    Email = nv.Email,
                    GioiTinh = nv.GioiTinh,
                    DiaChi = nv.DiaChi,
                    NgaySinh = nv.NgaySinh
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách: " + ex.Message });
            }
        }

        [HttpGet("get-by-name")]
        [Authorize(Roles = "Quản lý")]
        public async Task<IActionResult> GetByNameNhanVien([FromQuery] string hoTen)
        {
            try
            {
                var accounts = await _repository.GetByNameNhanVien(hoTen);
                var result = accounts.Select(nv => new Account
                {
                    HoTen = nv.HoTen,
                    MaVaiTro = nv.MaVaiTro,
                    SoDienThoai = nv.SoDienThoai,
                    Email = nv.Email,
                    GioiTinh = nv.GioiTinh,
                    DiaChi = nv.DiaChi,
                    NgaySinh = nv.NgaySinh
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi tìm kiếm: " + ex.Message });
            }
        }

        [HttpPost("add")]
        [Authorize(Roles = "Quản lý")]
        public async Task<IActionResult> AddAccount([FromBody] Account model)
        {
            try
            {
                var nhanVien = new NhanVien
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    MaVaiTro = model.MaVaiTro,
                    GioiTinh = model.GioiTinh,
                    DiaChi = model.DiaChi,
                    NgaySinh = model.NgaySinh
                };

                var addedAccount = await _repository.AddAccount(nhanVien);
                return Ok(new { Message = "Thêm tài khoản thành công!", Email = addedAccount.Email });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi thêm tài khoản: " + ex.Message });
            }
        }

        [HttpPut("update/{email}")]
        [Authorize(Roles = "Quản lý")]
        public async Task<IActionResult> UpdateAccount(string email, [FromBody] Account model)
        {
            try
            {
                var nhanVien = new NhanVien
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    MaVaiTro = model.MaVaiTro,
                    GioiTinh = model.GioiTinh,
                    DiaChi = model.DiaChi,
                    NgaySinh = model.NgaySinh
                };

                var success = await _repository.UpdateAccount(email, nhanVien);
                if (!success)
                {
                    return NotFound(new { Message = "Không tìm thấy tài khoản để cập nhật." });
                }

                return Ok(new { Message = "Cập nhật tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi cập nhật tài khoản: " + ex.Message });
            }
        }

        [HttpDelete("delete/{email}")]
        [Authorize(Roles = "Quản lý")]
        public async Task<IActionResult> DeleteAccount(string email)
        {
            try
            {
                var success = await _repository.DeleteAccount(email);
                if (!success)
                {
                    return NotFound(new { Message = "Không tìm thấy tài khoản hoặc tài khoản đã bị vô hiệu hóa." });
                }

                return Ok(new { Message = "Vô hiệu hóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi vô hiệu hóa tài khoản: " + ex.Message });
            }
        }

        [HttpPut("restore/{email}")]
        public async Task<IActionResult> RestoreAccount(string email)
        {
            try
            {
                var success = await _repository.RestoreAccount(email);
                if (!success)
                {
                    return NotFound(new { Message = "Không tìm thấy tài khoản hoặc tài khoản đã hoạt động." });
                }

                return Ok(new { Message = "Khôi phục tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi khôi phục tài khoản: " + ex.Message });
            }
        }

    }
}