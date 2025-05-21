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
        [HttpGet]
        public IActionResult GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var loaiPhongs = _loaiPhongRepository.GetAll(pageNumber, pageSize);
            return Ok(new { message = "Thành công", statusCode = 200, data = loaiPhongs });
        }

        [Authorize(Roles = "NhanVien")]
        [HttpGet("maLoaiPhong")]
        public IActionResult GetById(int maLoaiPhong)
        {
            var result = _loaiPhongRepository.GetById(maLoaiPhong);
            return Ok(new { message = "Thành công", statusCode = 200, data = result });
        }

        [Authorize(Roles = "QuanLy")]
        [HttpPost]
        public IActionResult AddLoaiPhong([FromBody] LoaiPhongVM loaiPhongVM)
        {
            if (loaiPhongVM == null)
            {
                return BadRequest(new { message = "Dữ liệu loại phòng không được để trống", statusCode = 400, data = (object)"" });
            }

            var result = _loaiPhongRepository.AddLoaiPhong(loaiPhongVM);
            return Ok(new { message = "Thêm loại phòng thành công", statusCode = 200, data = result });
        }

        [Authorize(Roles = "QuanLy,NhanVien")]
        [HttpPut("{maLoaiPhong}")]
        public IActionResult EditLoaiPhong(int maLoaiPhong, [FromBody] LoaiPhongVM loaiPhongVM)
        {
            if (loaiPhongVM == null)
            {
                return BadRequest(new { message = "Dữ liệu loại phòng không được để trống", statusCode = 400, data = (object)"" });
            }

            var result = _loaiPhongRepository.EditLoaiPhong(maLoaiPhong, loaiPhongVM);
            return Ok(new { message = "Cập nhật loại phòng thành công", statusCode = 200, data = result });
        }

        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{maLoaiPhong}")]
        public IActionResult DeleteLoaiPhong(int maLoaiPhong)
        {
            var result = _loaiPhongRepository.DeleteLoaiPhong(maLoaiPhong);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy loại phòng để xóa.", statusCode = 404, data = "" });
            }

            return Ok(new { message = "Xóa loại phòng thành công", statusCode = 200, data = result });
        }
    }
}
