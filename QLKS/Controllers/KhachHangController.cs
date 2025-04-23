using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKS.Data;
using QLKS.Models;
using QLKS.Repository;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KhachHangController : ControllerBase
    {
        private readonly IKhachHangRepository _khachHangRepository;

        public KhachHangController(IKhachHangRepository khachHangRepository)
        {
            _khachHangRepository = khachHangRepository;
        }
        [Authorize(Roles = "NhanVien")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllKhachHang()
        {
            try
            {
                var khachHangs = await _khachHangRepository.GetAllKhachHang();
                return Ok(khachHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách khách hàng: " + ex.Message });
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpGet("get-by-name")]
        public async Task<IActionResult> GetKhachHangByName([FromQuery] string hoTen)
        {
            try
            {
                if (string.IsNullOrEmpty(hoTen))
                {
                    return BadRequest(new { Message = "Họ tên không được để trống." });
                }

                var khachHangs = await _khachHangRepository.GetKhachHangByName(hoTen);
                if (khachHangs == null || !khachHangs.Any())
                {
                    return NotFound(new { Message = "Không tìm thấy khách hàng nào với tên này." });
                }

                return Ok(khachHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi tìm khách hàng: " + ex.Message });
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpPost("add")]
        public async Task<IActionResult> AddKhachHang([FromBody] KhachHangVM model)
        {
            try
            {
                var khachHangVM = await _khachHangRepository.AddKhachHang(model);
                return Ok(new { Message = "Thêm khách hàng thành công!", Data = khachHangVM });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi thêm khách hàng: " + ex.Message });
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpPut("update/{hoTen}")]
        public async Task<IActionResult> UpdateKhachHang(string hoTen, [FromBody] KhachHangVM model)
        {
            try
            {
                var result = await _khachHangRepository.UpdateKhachHang(hoTen, model);
                if (!result)
                {
                    return NotFound(new { Message = "Không tìm thấy khách hàng để cập nhật." });
                }

                return Ok(new { Message = "Cập nhật khách hàng thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi cập nhật khách hàng: " + ex.Message });
            }
        }
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("delete/{hoTen}")]
        public async Task<IActionResult> DeleteKhachHang(string hoTen)
        {
            try
            {
                var result = await _khachHangRepository.DeleteKhachHang(hoTen);
                if (!result)
                {
                    return NotFound(new { Message = "Không tìm thấy khách hàng để xóa." });
                }

                return Ok(new { Message = "Xóa khách hàng thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi xóa khách hàng: " + ex.Message });
            }
        }
    }
}
