using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using QLKS.Data;
using QLKS.Models;
using QLKS.Repository;
using Microsoft.AspNetCore.Authorization;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhongController : Controller
    {
        private readonly IPhongRepository _phong;

        public PhongController(IPhongRepository phong)
        {
            _phong = phong;
        }
        [Authorize(Roles = "NhanVien")]
        [HttpGet]
        public IActionResult GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var phong = _phong.GetAll(pageNumber, pageSize);
            return Ok(phong);
        }


        [Authorize(Roles = "NhanVien")]
        [HttpGet("{MaPhong}")]
        public IActionResult GetById(string MaPhong)
        {
            return Ok(_phong.GetById(MaPhong));
        }


        [Authorize(Roles = "QuanLy")]
        [HttpPost]
        public IActionResult AddPhong(PhongMD phongVM)
        {
            var phong = _phong.AddPhong(phongVM);
            return Ok(phong);
        }


       [Authorize(Roles = "QuanLy,NhanVien")]
        [HttpPut("{MaPhong}")]
        public IActionResult EditPhong(string MaPhong, PhongVM phongVM)
        {
            return Ok(_phong.EditPhong(MaPhong, phongVM));
        }


        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{MaPhong}")]
        public IActionResult DeletePhong(string MaPhong)
        {
            return Ok(_phong.DeletePhong(MaPhong));
        }


        [Authorize(Roles = "NhanVien")]
        [HttpGet("trang-thai/{trangThai}")]
        public IActionResult GetByTrangThai(string trangThai, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(trangThai))
            {
                return BadRequest("TrangThai không thể bỏ trống");
            }

            var phongList = _phong.GetByTrangThai(trangThai, pageNumber, pageSize);
            if (phongList.Phongs == null || !phongList.Phongs.Any())
            {
                return NotFound("Không tìm thấy phòng nào với trạng thái được chỉ định");
            }

            return Ok(phongList);
        }


        [Authorize(Roles = "NhanVien")]
        [HttpPut("{maPhong}/trang-thai")]
        public IActionResult UpdateTrangThai(string maPhong, [FromQuery] string trangThai)
        {
            if (string.IsNullOrEmpty(trangThai))
            {
                return BadRequest("TrangThai không thể bỏ trống");
            }

            var result = _phong.UpdateTrangThai(maPhong, trangThai);
            return Ok(result);
        }


        [Authorize(Roles = "NhanVien")]
        [HttpGet("loai-phong/{maLoaiPhong}")]
        public IActionResult GetByLoaiPhong(int maLoaiPhong, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var phongList = _phong.GetByLoaiPhong(maLoaiPhong, pageNumber, pageSize);
            if (phongList.Phongs == null || !phongList.Phongs.Any())
            {
                return NotFound("Không tìm thấy phòng có mã loại phòng đã nhập");
            }

            return Ok(phongList);
        }


        [Authorize(Roles = "QuanLy,NhanVien")]
        [HttpGet("thong-ke-trang-thai")]
        public IActionResult GetRoomStatusStatistics()
        {
            var statistics = _phong.GetRoomStatusStatistics();
            return Ok(statistics);
        }


        [Authorize(Roles = "NhanVien")]
        [HttpGet("{maPhong}/trang-thai-dat-phong")]
        public IActionResult IsRoomAvailable(string maPhong, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Ngày bắt đầu phải trước ngày kết thúc");
            }

            // Kiểm tra xem phòng có tồn tại không
            var phong = _phong.GetById(maPhong);
            if (phong.Value == null) // Kiểm tra nếu phòng không tồn tại
            {
                return NotFound(new { MaPhong = maPhong, TrangThai = "Phòng không tồn tại" });
            }

            var isAvailable = _phong.IsRoomAvailable(maPhong, startDate, endDate);
            var trangThai = isAvailable ? "Phòng trống" : "Phòng đã được đặt";

            return Ok(new { MaPhong = maPhong, TrangThai = trangThai });
        }
    }
}
