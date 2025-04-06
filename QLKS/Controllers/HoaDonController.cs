// HoaDonController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonRepository _hoaDonRepository;

        public HoaDonController(IHoaDonRepository hoaDonRepository)
        {
            _hoaDonRepository = hoaDonRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HoaDonVM>>> GetAll()
        {
            try
            {
                var hoaDons = await _hoaDonRepository.GetAllAsync();
                return Ok(hoaDons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet("{maHoaDon}")]
        public async Task<ActionResult<HoaDonVM>> GetById(int maHoaDon)
        {
            try
            {
                var hoaDon = await _hoaDonRepository.GetByIdAsync(maHoaDon);
                return Ok(hoaDon);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet("KhachHang/{maKh}")]
        public async Task<ActionResult<IEnumerable<HoaDonVM>>> GetByMaKh(int maKh)
        {
            try
            {
                var hoaDons = await _hoaDonRepository.GetByMaKhAsync(maKh);
                return Ok(hoaDons);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

       // [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost("Create")]
        public async Task<ActionResult<HoaDonVM>> Create([FromBody] CreateHoaDonVM hoaDonVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newHoaDon = await _hoaDonRepository.AddAsync(hoaDonVM);
                return CreatedAtAction(nameof(GetById), new { maHoaDon = newHoaDon.MaHoaDon }, newHoaDon);
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

       // [Authorize(Roles = "Admin,NhanVien")]
        [HttpPut("Update/{maHoaDon}")]
        public async Task<ActionResult<HoaDonVM>> Update(int maHoaDon, [FromBody] UpdateHoaDonVM hoaDonVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedHoaDon = await _hoaDonRepository.UpdateAsync(maHoaDon, hoaDonVM);
                return Ok(updatedHoaDon);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

       // [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{maHoaDon}")]
        public async Task<IActionResult> Delete(int maHoaDon)
        {
            try
            {
                var result = await _hoaDonRepository.DeleteAsync(maHoaDon);
                if (!result)
                    return NotFound("Hóa đơn không tồn tại.");

                return Ok("Xóa hóa đơn thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }
        //[Authorize(Roles = "Admin,NhanVien")]
        [HttpPost("ThanhToan/{maHoaDon}")]
        public async Task<ActionResult<HoaDonVM>> ThanhToan(int maHoaDon, [FromQuery] string phuongThucThanhToan)
        {
            try
            {
                var hoaDon = await _hoaDonRepository.ThanhToanAsync(maHoaDon, phuongThucThanhToan);
                return Ok(hoaDon);
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
        [HttpGet("TrangThai/{trangThai}")]
        public async Task<ActionResult<IEnumerable<HoaDonVM>>> GetByTrangThai(string trangThai)
        {
            try
            {
                var hoaDons = await _hoaDonRepository.GetByTrangThaiAsync(trangThai);
                return Ok(hoaDons);
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

    }
}
