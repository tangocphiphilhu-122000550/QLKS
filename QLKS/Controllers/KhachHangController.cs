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
        [HttpGet]
        public async Task<IActionResult> GetAllKhachHang([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var khachHangs = await _khachHangRepository.GetAllKhachHang(pageNumber, pageSize);
                return Ok(new { message = "Thành công", statusCode = 200, data = khachHangs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách khách hàng", statusCode = 500, data = ex.Message });
            }
        }


        [Authorize(Roles = "NhanVien")]
        [HttpGet("{hoTen}")]
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
        [HttpPost]
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
        [HttpPut("{hoTen}")]
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
        [HttpDelete("{hoTen}")]
        public async Task<IActionResult> DeleteKhachHang(string hoTen)
        {
            try
            {
                var result = await _khachHangRepository.DeleteKhachHang(hoTen);
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy khách hàng để xóa.", statusCode = 404, data = "" });
                }

                return Ok(new { message = "Xóa khách hàng thành công!", statusCode = 200, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa khách hàng: " + ex.Message, statusCode = 500, data = "" });
            }
        }
    }
}
