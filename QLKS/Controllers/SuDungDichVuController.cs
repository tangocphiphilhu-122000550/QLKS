using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SuDungDichVuController : ControllerBase
    {
        private readonly ISuDungDichVuRepository _suDungDichVuRepository;

        public SuDungDichVuController(ISuDungDichVuRepository suDungDichVuRepository)
        {
            _suDungDichVuRepository = suDungDichVuRepository;
        }


        [Authorize(Roles = "NhanVien")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllSuDungDichVu([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var suDungDichVus = await _suDungDichVuRepository.GetAllSuDungDichVu(pageNumber, pageSize);
                return Ok(suDungDichVus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách sử dụng dịch vụ: " + ex.Message });
            }
        }


        [Authorize(Roles = "NhanVien")]
        [HttpPost("add")]
        public async Task<IActionResult> AddSuDungDichVu([FromBody] CreateSuDungDichVuVM model)
        {
            try
            {
                var result = await _suDungDichVuRepository.AddSuDungDichVu(model);
                if (!result)
                {
                    return BadRequest(new { Message = "Không thể tạo sử dụng dịch vụ." });
                }
                return Ok(new { Message = "Tạo thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi thêm sử dụng dịch vụ: " + ex.Message });
            }
        }


        [Authorize(Roles = "NhanVien")]
        [HttpPut("update/{maSuDung}")]
        public async Task<IActionResult> UpdateSuDungDichVu(int maSuDung, [FromBody] SuDungDichVuVM model)
        {
            try
            {
                var result = await _suDungDichVuRepository.UpdateSuDungDichVu(maSuDung, model);
                if (!result)
                {
                    return NotFound(new { Message = "Không tìm thấy bản ghi sử dụng dịch vụ để cập nhật." });
                }

                return Ok(new { Message = "Cập nhật sử dụng dịch vụ thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi cập nhật sử dụng dịch vụ: " + ex.Message });
            }
        }


        [Authorize(Roles = "QuanLy,NhanVien")]
        [HttpDelete("delete/{maSuDung}")]
        public async Task<IActionResult> DeleteSuDungDichVu(int maSuDung)
        {
            try
            {
                var result = await _suDungDichVuRepository.DeleteSuDungDichVu(maSuDung);
                if (!result)
                {
                    return NotFound(new { Message = "Không tìm thấy bản ghi sử dụng dịch vụ để xóa." });
                }

                return Ok(new { Message = "Xóa sử dụng dịch vụ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi xóa sử dụng dịch vụ: " + ex.Message });
            }
        }
    }
}
