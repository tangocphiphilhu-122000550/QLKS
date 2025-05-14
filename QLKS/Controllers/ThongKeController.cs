using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QLKS.Repository;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly IThongKeRepository _thongKeRepository;
        public ThongKeController(IThongKeRepository thongKeRepository)
        {
            _thongKeRepository = thongKeRepository;
        }

        [Authorize(Roles = "QuanLy")]
        [HttpGet("by-date")]
        public async Task<IActionResult> ThongKeTheoNgay([FromQuery] DateTime ngay)
        {
            if (ngay == default)
            {
                return BadRequest(new { Message = "Ngày không được để trống." });
            }

            var result = await _thongKeRepository.ThongKeTheoNgay(ngay);
            return Ok(result);
        }

        [Authorize(Roles = "QuanLy")]
        [HttpGet("TheoKhoangThoiGian")]
        public async Task<IActionResult> ThongKeTheoKhoangThoiGian([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            if (tuNgay > denNgay)
            {
                return BadRequest("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            var result = await _thongKeRepository.ThongKeTheoKhoangThoiGian(tuNgay, denNgay);
            return Ok(result);
        }

        [Authorize(Roles = "QuanLy")]
        [HttpGet("by-month")]
        public async Task<IActionResult> ThongKeTheoThang([FromQuery] int nam, [FromQuery] int thang)
        {
            if (nam < 2000 || nam > DateTime.Now.Year || thang < 1 || thang > 12)
            {
                return BadRequest(new { Message = "Năm hoặc tháng không hợp lệ." });
            }

            var result = await _thongKeRepository.ThongKeTheoThang(nam, thang);
            return Ok(result);
        }

        [Authorize(Roles = "QuanLy")]
        [HttpGet("by-year")]
        public async Task<IActionResult> ThongKeTheoNam([FromQuery] int nam)
        {
            if (nam < 2000 || nam > DateTime.Now.Year)
            {
                return BadRequest(new { Message = "Năm không hợp lệ." });
            }

            var result = await _thongKeRepository.ThongKeTheoNam(nam);
            return Ok(result);
        }
    }
}

