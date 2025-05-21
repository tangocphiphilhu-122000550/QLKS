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
        public async Task<ActionResult<PagedDatPhongResponse>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _datPhongRepository.GetAllVMAsync(pageNumber, pageSize);
                return Ok(new { message = "Thành công", statusCode = 200, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đặt phòng", statusCode = 500, data = ex.Message });
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
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateDatPhongRequest request)
        {
            if (request == null || request.DatPhongVMs == null || !request.DatPhongVMs.Any())
                return BadRequest("Danh sách đặt phòng không được để trống.");

            try
            {
                await _datPhongRepository.AddVMAsync(request.DatPhongVMs, request.MaKhList);
                return Ok("Thêm đặt phòng thành công");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPut("{maDatPhong}")]
        public async Task<ActionResult> Update(int maDatPhong, [FromBody] UpdateDatPhongVM datPhongVM)
        {
            if (datPhongVM == null)
                return BadRequest("Dữ liệu cập nhật không được để trống.");

            try
            {
                await _datPhongRepository.UpdateVMAsync(maDatPhong, datPhongVM);
                return Ok("Cập nhật thành công");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{maDatPhong}")]
        public async Task<ActionResult> Delete(int maDatPhong)
        {
            try
            {
                var result = await _datPhongRepository.DeleteByMaDatPhongAsync(maDatPhong);
                if (!result)
                    return NotFound(new { message = "Đặt phòng không tồn tại hoặc đã bị xóa.", statusCode = 404, data = (object)null });
                return Ok(new { message = "Xóa thành công", statusCode = 200, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi server: " + ex.Message, statusCode = 400, data = (object)null });
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPut("rooms/{maPhong}/status")]
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

    // Class để nhận dữ liệu từ request khi tạo đặt phòng
    public class CreateDatPhongRequest
    {
        public List<CreateDatPhongVM> DatPhongVMs { get; set; }
        public List<int> MaKhList { get; set; }
    }
}