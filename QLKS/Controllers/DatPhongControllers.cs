using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;
using System;
using System.Threading.Tasks;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatPhongController : ControllerBase
    {
        private readonly IDatPhongRepository _datPhongRepository;

        public DatPhongController(IDatPhongRepository datPhongRepository)
        {
            _datPhongRepository = datPhongRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var datPhongs = await _datPhongRepository.GetAllVMAsync();
                return Ok(datPhongs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{maDatPhong}")]
        public async Task<IActionResult> GetById(int maDatPhong)
        {
            try
            {
                var datPhong = await _datPhongRepository.GetByIdVMAsync(maDatPhong);
                if (datPhong == null)
                    return NotFound("Đặt phòng không tồn tại.");

                return Ok(datPhong);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateDatPhongVM datPhongVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _datPhongRepository.AddVMAsync(datPhongVM);
                return Ok("Create thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("Update/{maDatPhong}")]
        public async Task<IActionResult> Update(int maDatPhong, [FromBody] UpdateDatPhongVM datPhongVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _datPhongRepository.UpdateVMAsync(maDatPhong, datPhongVM);
                return Ok("Update thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{maDatPhong}")]
        public async Task<IActionResult> Delete(int maDatPhong)
        {
            try
            {
                var result = await _datPhongRepository.DeleteByMaDatPhongAsync(maDatPhong);
                if (!result)
                    return NotFound("Đặt phòng không tồn tại.");

                return Ok("Delete thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}