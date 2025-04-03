using Microsoft.AspNetCore.Mvc;
using QLKS.Repository;
using QLKS.Data;
using QLKS.Models; 
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq; 
using Microsoft.EntityFrameworkCore; 

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatPhongController : ControllerBase
    {
        private const int V = 0;
        private readonly IDatPhongRepository _datPhong;
        public DatPhongController(IDatPhongRepository datPhong )
        {
            _datPhong = datPhong;
        }

        // GET: api/DatPhong/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetAll()
        {
            var datPhongEntities = await _datPhong.GetAllAsync(); 

            // Ánh x? t? Entity sang ViewModel
            var datPhongVMs = datPhongEntities.Select(dp => new DatPhongVM
            {
                MaDatPhong = dp.MaDatPhong,
                MaNv = dp.MaNv,
                MaKh = dp.MaKh,
                MaPhong = dp.MaPhong,
                NgayDat = dp.NgayDat,
                NgayNhanPhong = dp.NgayNhanPhong,
                NgayTraPhong = dp.NgayTraPhong,
                SoNguoiO = dp.SoNguoiO,
                PhuThu = dp.PhuThu,
                TrangThai = dp.TrangThai,
                TongTienPhong = dp.TongTienPhong,
                SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0
            });

            return Ok(datPhongVMs);
        }

        // GET: api/DatPhong/GetById/5
        [HttpGet("GetByMaDatPhong/{id}")] 
        public async Task<ActionResult<DatPhongVM>> GetById(int id)
        {
            var dp = await _datPhong.GetByIdAsync(id);
            if (dp == null)
            {
                return NotFound();
            }
            var datPhongVM = new DatPhongVM
            {
                MaDatPhong = dp.MaDatPhong,
                MaNv = dp.MaNv,
                MaKh = dp.MaKh,
                MaPhong = dp.MaPhong,
                NgayDat = dp.NgayDat,
                NgayNhanPhong = dp.NgayNhanPhong,
                NgayTraPhong = dp.NgayTraPhong,
                SoNguoiO = dp.SoNguoiO,
                PhuThu = dp.PhuThu,
                TrangThai = dp.TrangThai,
                TongTienPhong = dp.TongTienPhong,
                SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0
            };
            return Ok(datPhongVM);
        }
        // GET: api/DatPhong/GetByMaPhong
        [HttpGet("GetByMaPhong/{maPhong}")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByMaPhong(string maPhong)
        {
            if (string.IsNullOrWhiteSpace(maPhong))
            {
                return BadRequest("Mã phòng không được để trống.");
            }
            try
            {
                var datPhongEntities = await _datPhong.GetByMaPhongAsync(maPhong);
                var datPhongVMs = datPhongEntities.Select(dp => new DatPhongVM
                {
                    MaDatPhong = dp.MaDatPhong,
                    MaNv = dp.MaNv,
                    MaKh = dp.MaKh,
                    MaPhong = dp.MaPhong,
                    NgayDat = dp.NgayDat,
                    NgayNhanPhong = dp.NgayNhanPhong,
                    NgayTraPhong = dp.NgayTraPhong,
                    SoNguoiO = dp.SoNguoiO,
                    PhuThu = dp.PhuThu,
                    TrangThai = dp.TrangThai,
                    TongTienPhong = dp.TongTienPhong,
                    SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0
                });
                return Ok(datPhongVMs);
            }
            catch (Microsoft.Data.SqlClient.SqlException )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi xảy ra khi truy vấn cơ sở dữ liệu.");
            }
            catch (Exception )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Đã có lỗi xảy ra trong quá trình xử lý yêu cầu.");
            }
        }
        // POST: api/DatPhong/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DatPhong datPhong)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(datPhong.MaPhong) ||
               datPhong.NgayNhanPhong == default ||
               datPhong.NgayTraPhong == default)
            {
                return BadRequest("Mã phòng, ngày nhận phòng và ngày trả phòng không được để trống.");
            }
            if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
            {
                return BadRequest("Ngày nhận phòng phải trước ngày trả phòng.");
            }
            var isPhongDat = await _datPhong.IsPhongDatAsync(
               datPhong.MaPhong,
               datPhong.NgayNhanPhong,
               datPhong.NgayTraPhong);

            if (isPhongDat)
            {
                return BadRequest("Phòng đã được đặt trong khoảng thời gian này.");
            }
            var createdDatPhong = await _datPhong.AddAsync(datPhong);
            var newDp = await _datPhong.GetByIdAsync(createdDatPhong.MaDatPhong);
            if (newDp == null)
            {
                return StatusCode(500, "Không thể truy xuất ??t phòng v?a t?o.");
            }

            var datPhongVM = new DatPhongVM
            {
                MaDatPhong = newDp.MaDatPhong,
                MaNv = newDp.MaNv,
                MaKh = newDp.MaKh,
                MaPhong = newDp.MaPhong,
                NgayDat = newDp.NgayDat,
                NgayNhanPhong = newDp.NgayNhanPhong,
                NgayTraPhong = newDp.NgayTraPhong,
                SoNguoiO = newDp.SoNguoiO,
                PhuThu = newDp.PhuThu,
                TrangThai = newDp.TrangThai,
                TongTienPhong = newDp.TongTienPhong,
                SoLuongDichVuSuDung = newDp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0
            };
            return CreatedAtAction(nameof(GetById), new { id = createdDatPhong.MaDatPhong }, datPhongVM);
        }
        // GET: api/DatPhong/GetByMaKh/5
        [HttpGet("GetByMaKh/{maKh}")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByMaKh(int maKh)
        {
            var datPhongs = await _datPhong.GetByMaKhAsync(maKh);
            var datPhongVMs = datPhongs.Select(dp => new DatPhongVM
            {
                MaDatPhong = dp.MaDatPhong,
                MaNv = dp.MaNv,
                MaKh = dp.MaKh,
                MaPhong = dp.MaPhong,
                NgayDat = dp.NgayDat,
                NgayNhanPhong = dp.NgayNhanPhong,
                NgayTraPhong = dp.NgayTraPhong,
                SoNguoiO = dp.SoNguoiO,
                PhuThu = dp.PhuThu,
                TrangThai = dp.TrangThai,
                TongTienPhong = dp.TongTienPhong,
                SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0
            });
            return Ok(datPhongVMs);
        }
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DatPhong datPhong)
        {
            if (id != datPhong.MaDatPhong)
            {
                // Nên s? d?ng ngôn ng?/mã hóa nh?t quán
                return BadRequest("ID không khớp với dữ liệu.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Logic ki?m tra h?p l? hi?n có c?a b?n ---
            if (string.IsNullOrEmpty(datPhong.MaPhong))
            {
                // Nên s? d?ng ngôn ng?/mã hóa nh?t quán
                return BadRequest("Mã phòng không được để trống.");
            }
            if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
            {
                return BadRequest("Ngày nhận phòng phải trước ngày trả phòng.");
            }
            try
            {
                await _datPhong.UpdateAsync(datPhong);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _datPhong.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/DatPhong/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _datPhong.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
