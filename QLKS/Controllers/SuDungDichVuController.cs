using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SuDungDichVuController : ControllerBase
    {
        private readonly ISuDungDichVuRepository _suDungDichVuRepository;

        public SuDungDichVuController(ISuDungDichVuRepository suDungDichVuRepository)
        {
            _suDungDichVuRepository = suDungDichVuRepository;
        }

        [HttpGet("get-all")]
        //[Authorize(Roles = "2")]
        public async Task<IActionResult> GetAllSuDungDichVu()
        {
            try
            {
                var suDungDichVus = await _suDungDichVuRepository.GetAllSuDungDichVu();
                return Ok(suDungDichVus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách sử dụng dịch vụ: " + ex.Message });
            }
        }

        [HttpPost("add")]
        //[Authorize(Roles = "2")]
        public async Task<IActionResult> AddSuDungDichVu([FromBody] SuDungDichVuVM model)
        {
            try
            {
                var suDungDichVuVM = await _suDungDichVuRepository.AddSuDungDichVu(model);
                return Ok(new { Message = "Thêm sử dụng dịch vụ thành công!", Data = suDungDichVuVM });
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

        [HttpPut("update/{maSuDung}")]
        //[Authorize(Roles = "2")]
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

        [HttpDelete("delete/{maSuDung}")]
        //[Authorize(Roles = "2")]
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