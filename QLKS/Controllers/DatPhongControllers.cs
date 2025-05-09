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
    [Authorize]
    public class DatPhongController : ControllerBase
    {
        private readonly IDatPhongRepository _datPhongRepository;

        public DatPhongController(IDatPhongRepository datPhongRepository)
        {
            _datPhongRepository = datPhongRepository;
        }
        [Authorize(Roles = "NhanVien")]
        [HttpGet]
        public async Task<ActionResult<List<DatPhongVM>>> GetAll()
        {
            try
            {
                var datPhongs = await _datPhongRepository.GetAllVMAsync();
                return Ok(datPhongs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpGet("{maDatPhong}")]
        public async Task<ActionResult<DatPhongVM>> GetById(int maDatPhong)
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
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] List<CreateDatPhongVM> datPhongVMs)
        {
            try
            {
                await _datPhongRepository.AddVMAsync(datPhongVMs);
                return Ok("Thêm đặt phòng thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpPut("Update/{maDatPhong}")]
        public async Task<ActionResult> Update(int maDatPhong, [FromBody] UpdateDatPhongVM datPhongVM)
        {
            try
            {
                await _datPhongRepository.UpdateVMAsync(maDatPhong, datPhongVM);
                return Ok("Update thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("Delete/{maDatPhong}")]
        public async Task<ActionResult> Delete(int maDatPhong)
        {
            try
            {
                var result = await _datPhongRepository.DeleteByMaDatPhongAsync(maDatPhong);
                if (!result)
                    return NotFound("Đặt phòng không tồn tại hoặc đã bị xóa.");
                return Ok("Xóa thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }
        [Authorize(Roles = "NhanVien")]
        [HttpPut("UpdatePhongTrangThai/{maPhong}")]

        public async Task<ActionResult> UpdatePhongTrangThai([FromRoute] string maPhong, [FromBody] string trangThai)
        {
            if (string.IsNullOrEmpty(maPhong))
                return BadRequest(new { error = "Mã phòng không được để trống." });

            if (string.IsNullOrEmpty(trangThai))
                return BadRequest(new { error = "Trạng thái không được để trống." });

            try
            {
                await _datPhongRepository.UpdateDatPhongTrangThaiByMaPhongAsync(maPhong, trangThai);
                return Ok(new { message = $"Cập nhật trạng thái đặt phòng cho phòng {maPhong} thành '{trangThai}' thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi server: {ex.Message}" });
            }
        }
    }

   
}
