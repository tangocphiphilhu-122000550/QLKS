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
    public class DichVuController : ControllerBase
    {
        private readonly IDichVuRepository _dichVuRepository;

        public DichVuController(IDichVuRepository dichVuRepository)
        {
            _dichVuRepository = dichVuRepository;
        }

        [HttpGet()]
        [Authorize(Roles = "NhanVien")]
        public async Task<IActionResult> GetAllDichVu([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var dichVus = await _dichVuRepository.GetAllDichVu(pageNumber, pageSize);
                return Ok(dichVus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách dịch vụ: " + ex.Message });
            }
        }


        [HttpGet("search")]
        [Authorize(Roles = "NhanVien")]
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

        [HttpPost]
        [Authorize(Roles = "QuanLy")]
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

        [HttpPut("{tenDichVu}")]
        [Authorize(Roles = "QuanLy")]
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

        [HttpDelete("{tenDichVu}")]
        [Authorize(Roles = "QuanLy")]
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
