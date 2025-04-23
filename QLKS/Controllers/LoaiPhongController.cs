using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;
using Microsoft.AspNetCore.Authorization;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoaiPhongController : ControllerBase
    {
        private readonly ILoaiPhongRepository _loaiPhongRepository;

        public LoaiPhongController(ILoaiPhongRepository loaiPhongRepository)
        {
            _loaiPhongRepository = loaiPhongRepository;
        }

        [Authorize(Roles = "NhanVien")]
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var loaiPhongs = _loaiPhongRepository.GetAll();
            return Ok(loaiPhongs);
        }

        [Authorize(Roles = "NhanVien")]
        [HttpGet("GetById")]
        public IActionResult GetById(int maLoaiPhong)
        {
            var result = _loaiPhongRepository.GetById(maLoaiPhong);
            return Ok(result);
        }

        [Authorize(Roles = "QuanLy")]
        [HttpPost("AddLoaiPhong")]
        public IActionResult AddLoaiPhong([FromBody] LoaiPhongVM loaiPhongVM)
        {
            if (loaiPhongVM == null)
            {
                return BadRequest("Dữ liệu loại phòng không được để trống");
            }

            var result = _loaiPhongRepository.AddLoaiPhong(loaiPhongVM);
            return Ok(result);
        }

        [Authorize(Roles = "QuanLy,NhanVien")]
        [HttpPut("EditLoaiPhong/{maLoaiPhong}")]
        public IActionResult EditLoaiPhong(int maLoaiPhong, [FromBody] LoaiPhongVM loaiPhongVM)
        {
            if (loaiPhongVM == null)
            {
                return BadRequest("Dữ liệu loại phòng không được để trống");
            }

            var result = _loaiPhongRepository.EditLoaiPhong(maLoaiPhong, loaiPhongVM);
            return Ok(result);
        }

        [Authorize(Roles = "QuanLy")]
        [HttpDelete("DeleteLoaiPhong/{maLoaiPhong}")]
        public IActionResult DeleteLoaiPhong(int maLoaiPhong)
        {
            var result = _loaiPhongRepository.DeleteLoaiPhong(maLoaiPhong);
            return Ok(result);
        }
    }
}
