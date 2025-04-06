// DatPhongController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;
using QLKS.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatPhongController : ControllerBase
    {
        private readonly IDatPhongRepository _datPhongRepository;
        private readonly DataQlks112Nhom3Context _context;

        public DatPhongController(IDatPhongRepository datPhongRepository, DataQlks112Nhom3Context context)
        {
            _datPhongRepository = datPhongRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetAll()
        {
            try
            {
                var datPhongs = await _datPhongRepository.GetAllVMAsync();
                return Ok(datPhongs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet("Phong/{maPhong}")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByMaPhong(string maPhong)
        {
            try
            {
                var datPhongs = await _datPhongRepository.GetByMaPhongVMAsync(maPhong);
                return Ok(datPhongs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet("KhachHang/{tenKhachHang}")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByTenKhachHang(string tenKhachHang)
        {
            try
            {
                var datPhongs = await _datPhongRepository.GetByTenKhachHangVMAsync(tenKhachHang);
                return Ok(datPhongs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet("DateRange")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByDateRange([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var datPhongs = await _datPhongRepository.GetByDateRangeAsync(startDate, endDate);
                return Ok(datPhongs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<DatPhongVM>> Create([FromBody] CreateDatPhongVM datPhongVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newDatPhong = await _datPhongRepository.AddVMAsync(datPhongVM);
                await _datPhongRepository.UpdateDatPhongStatusAsync();
                return CreatedAtAction(nameof(GetByMaPhong), new { maPhong = newDatPhong.MaPhong }, newDatPhong);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPut("Update/{maDatPhong}")]
        public async Task<ActionResult<DatPhongVM>> Update(int maDatPhong, [FromBody] UpdateDatPhongVM datPhongVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedDatPhong = await _datPhongRepository.UpdateVMAsync(maDatPhong, datPhongVM);
                await _datPhongRepository.UpdateDatPhongStatusAsync();
                return Ok(updatedDatPhong);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("Delete/{maDatPhong}")]
        public async Task<IActionResult> Delete(int maDatPhong)
        {
            try
            {
                var datPhong = await _context.DatPhongs.FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong);
                if (datPhong == null)
                    return NotFound("Đặt phòng không tồn tại.");

                var result = await _datPhongRepository.DeleteByMaDatPhongAsync(maDatPhong);
                if (!result)
                    return NotFound("Đặt phòng không tồn tại.");

                await _datPhongRepository.UpdateDatPhongStatusAsync();
                return Ok($"Xóa đặt phòng thành công cho phòng {datPhong.MaPhong}.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("UpdateDatPhongStatus")]
        public async Task<IActionResult> UpdateDatPhongStatus()
        {
            try
            {
                await _datPhongRepository.UpdateDatPhongStatusAsync();
                return Ok("Đã cập nhật trạng thái đặt phòng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("Cancel/{maDatPhong}")]
        public async Task<IActionResult> Cancel(int maDatPhong)
        {
            try
            {
                await _datPhongRepository.CancelDatPhongAsync(maDatPhong);
                await _datPhongRepository.UpdateDatPhongStatusAsync();
                return Ok($"Hủy đặt phòng {maDatPhong} thành công.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }
    }
}
