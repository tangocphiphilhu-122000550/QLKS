using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKS.Data;
using QLKS.Models;
using QLKS.Repository;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DichVuController : ControllerBase
    {
        private readonly IDichVuRepository _dichVuRepository;

        public DichVuController(IDichVuRepository dichVuRepository)
        {
            _dichVuRepository = dichVuRepository;
        }

        [HttpGet("get-all")]
        ////[Authorize(Roles = "2")] // Chỉ Quản lý được xem
        public async Task<IActionResult> GetAllDichVu()
        {
            try
            {
                var dichVus = await _dichVuRepository.GetAllDichVu();
                return Ok(dichVus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách dịch vụ: " + ex.Message });
            }
        }

        [HttpGet("get-by-name")]
        //[Authorize(Roles = "2")] // Chỉ Quản lý được xem
        public async Task<IActionResult> GetDichVuByName([FromQuery] string tenDichVu)
        {
            try
            {
                if (string.IsNullOrEmpty(tenDichVu))
                {
                    return BadRequest(new { Message = "Tên dịch vụ không được để trống." });
                }

                var dichVus = await _dichVuRepository.GetDichVuByName(tenDichVu);
                if (dichVus == null || !dichVus.Any())
                {
                    return NotFound(new { Message = "Không tìm thấy dịch vụ nào với tên này." });
                }

                return Ok(dichVus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi tìm dịch vụ: " + ex.Message });
            }
        }

        [HttpPost("add")]
        //[Authorize(Roles = "2")]
        public async Task<IActionResult> AddDichVu([FromBody] DichVuVM model)
        {
            try
            {
                var dichVuVM = await _dichVuRepository.AddDichVu(model);
                return Ok(new { Message = "Thêm dịch vụ thành công!", Data = dichVuVM });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi thêm dịch vụ: " + ex.Message });
            }
        }

        [HttpPut("update/{tenDichVu}")]
        //[Authorize(Roles = "2")]
        public async Task<IActionResult> UpdateDichVu(string tenDichVu, [FromBody] DichVuVM model)
        {
            try
            {
                var result = await _dichVuRepository.UpdateDichVu(tenDichVu, model);
                if (!result)
                {
                    return NotFound(new { Message = "Không tìm thấy dịch vụ để cập nhật." });
                }

                return Ok(new { Message = "Cập nhật dịch vụ thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi cập nhật dịch vụ: " + ex.Message });
            }
        }

        [HttpDelete("delete/{tenDichVu}")]
        //[Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteDichVu(string tenDichVu)
        {
            try
            {
                var result = await _dichVuRepository.DeleteDichVu(tenDichVu);
                if (!result)
                {
                    return NotFound(new { Message = "Không tìm thấy dịch vụ để xóa." });
                }

                return Ok(new { Message = "Xóa dịch vụ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi xóa dịch vụ: " + ex.Message });
            }
        }
    }
}