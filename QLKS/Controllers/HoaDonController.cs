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
    [Authorize]
    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonRepository _hoaDonRepository;

        public HoaDonController(IHoaDonRepository hoaDonRepository)
        {
            _hoaDonRepository = hoaDonRepository;
        }
        [Authorize(Roles = "NhanVien")]
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
        [Authorize(Roles = "NhanVien")]
        [HttpGet("TenKhachHang/{tenKhachHang}")]
        public async Task<ActionResult<IEnumerable<HoaDonVM>>> GetByTenKhachHang(string tenKhachHang)
        {
            try
            {
                var hoaDons = await _hoaDonRepository.GetByTenKhachHangAsync(tenKhachHang);
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
        [Authorize(Roles = "NhanVien")]
        [HttpPost("Create")]
        public async Task<ActionResult<HoaDonVM>> Create([FromBody] CreateHoaDonVM hoaDonVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newHoaDon = await _hoaDonRepository.CreateAsync(hoaDonVM);
                return CreatedAtAction(nameof(GetAll), newHoaDon);
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
        [Authorize(Roles = "NhanVien")]
        [HttpPut("UpdateTrangThaiByTenKhachHang/{tenKhachHang}")]
        public async Task<IActionResult> UpdateTrangThaiByTenKhachHang(string tenKhachHang, [FromBody] UpdateHoaDonVM updateVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hoaDonRepository.UpdateTrangThaiByTenKhachHangAsync(tenKhachHang, updateVM);
                if (!result)
                    return NotFound($"Không tìm thấy hóa đơn nào cho tên khách hàng: {tenKhachHang}");

                return Ok("Cập nhật trạng thái thành công.");
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
        [Authorize(Roles = "NhanVien")]
        [HttpPut("UpdatePhuongThucThanhToanByTenKhachHang/{tenKhachHang}")]
        public async Task<IActionResult> UpdatePhuongThucThanhToanByTenKhachHang(string tenKhachHang, [FromBody] UpdatePhuongThucThanhToanVM updateVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hoaDonRepository.UpdatePhuongThucThanhToanByTenKhachHangAsync(tenKhachHang, updateVM);
                if (!result)
                    return NotFound($"Không tìm thấy hóa đơn nào cho tên khách hàng: {tenKhachHang}");

                return Ok("Cập nhật phương thức thanh toán thành công.");
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
