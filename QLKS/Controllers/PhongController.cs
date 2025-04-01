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
   // [Authorize]
    public class PhongController : Controller
    {
        private readonly IPhongRepository _phong;

        public PhongController(IPhongRepository phong)
        {
            _phong = phong;
        }
       // [Authorize(Roles = "Admin,NhanVien,SinhVien")]
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var phong = _phong.GetAll();
            return Ok(phong);
        }
       // [Authorize(Roles = "Admin,NhanVien")]
        [HttpGet("GetById")]
        public IActionResult GetById(string MaPhong)
        {
            return Ok(_phong.GetById(MaPhong));
        }
      //  [Authorize(Roles = "Admin")]
        [HttpPost("AddPhong")]
        public IActionResult AddPhong(PhongMD phongVM)
        {
            var phong = _phong.AddPhong(phongVM);
            return Ok(phong);
        }
       // [Authorize(Roles = "Admin,NhanVien")]
        [HttpPut("EditPhong/{MaPhong}")]
        public IActionResult EditPhong(string MaPhong, PhongVM phongVM)
        {
            return Ok(_phong.EditPhong(MaPhong, phongVM));
        }
        //[Authorize(Roles = "Admin")]
        [HttpDelete("DeletePhong/{MaPhong}")]
        public IActionResult DeletePhong(string MaPhong)
        {
            return Ok(_phong.DeletePhong(MaPhong));
        }

    }
}