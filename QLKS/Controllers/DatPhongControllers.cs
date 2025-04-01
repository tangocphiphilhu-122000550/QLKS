using Microsoft.AspNetCore.Mvc;
using QLKS.Repository;
using QLKS.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatPhongController : ControllerBase
    {
        private readonly IDatPhongRepository _datPhong;

        public DatPhongController(IDatPhongRepository datPhong)
        {
            _datPhong = datPhong;
        }

        // GET: api/DatPhong
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll() 
        {
            var datPhong = await _datPhong.GetAllAsync(); 
            return Ok(datPhong);
        }

        // GET: api/DatPhong/5
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int id)
        {
            var datPhong = await _datPhong.GetByIdAsync(id);
            if (datPhong == null)
            {
                return NotFound();
            }
            return Ok(datPhong);
        }

        // POST: api/DatPhong
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DatPhong datPhong)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ki?m tra c�c gi� tr? nullable
            if (string.IsNullOrEmpty(datPhong.MaPhong) ||
                datPhong.NgayNhanPhong == default ||
                datPhong.NgayTraPhong == default)
            {
                return BadRequest("M� ph�ng, ng�y nh?n ph�ng v� ng�y tr? ph�ng kh�ng ???c ?? tr?ng");
            }

            // Ki?m tra ph�ng ?� ???c ??t ch?a
            var isPhongDat = await _datPhong.IsPhongDatAsync(
                datPhong.MaPhong,
                datPhong.NgayNhanPhong,
                datPhong.NgayTraPhong);

            if (isPhongDat)
            {
                return BadRequest("Ph�ng ?� ???c ??t trong kho?ng th?i gian n�y");
            }

            var createdDatPhong = await _datPhong.AddAsync(datPhong);
            return CreatedAtAction(nameof(GetById), new { id = createdDatPhong.MaDatPhong }, createdDatPhong);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(int id, [FromBody] DatPhong datPhong)
        {
            if (id != datPhong.MaDatPhong)
            {
                return BadRequest("ID kh�ng kh?p v?i d? li?u");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingDatPhong = await _datPhong.GetByIdAsync(id);
            if (existingDatPhong == null)
            {
                return NotFound();
            }

            // Ki?m tra MaPhong c� null kh�ng
            if (string.IsNullOrEmpty(datPhong.MaPhong))
            {
                return BadRequest("M� ph�ng kh�ng ???c ?? tr?ng");
            }

            // Ki?m tra n?u c� thay ??i th?i gian ho?c m� ph�ng
            if (existingDatPhong.MaPhong != datPhong.MaPhong ||
                existingDatPhong.NgayNhanPhong != datPhong.NgayNhanPhong ||
                existingDatPhong.NgayTraPhong != datPhong.NgayTraPhong)
            {
                // Ki?m tra ng�y h?p l?
                if (datPhong.NgayNhanPhong > datPhong.NgayTraPhong)
                {
                    return BadRequest("Ng�y nh?n ph�ng ph?i tr??c ng�y tr? ph�ng");
                }

                var isPhongDat = await _datPhong.IsPhongDatAsync(
                    datPhong.MaPhong,
                    datPhong.NgayNhanPhong,
                    datPhong.NgayTraPhong);

                if (isPhongDat)
                {
                    return BadRequest("Ph�ng ?� ???c ??t trong kho?ng th?i gian n�y");
                }
            }

            await _datPhong.UpdateAsync(datPhong);
            return NoContent();
        }

        // DELETE: api/DatPhong/5
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _datPhong.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET: api/DatPhong/khachhang/5
        [HttpGet("GetByMaKh")]
        public async Task<IActionResult> GetByMaKh(int maKh)
        {
            var datPhongs = await _datPhong.GetByMaKhAsync(maKh);
            return Ok(datPhongs);
        }

        // GET: api/DatPhong/nhanvien/1
        [HttpGet("GetByMaNv")]
        public async Task<IActionResult> GetByMaNv(int maNv)
        {
            var datPhongs = await _datPhong.GetByMaNvAsync(maNv);
            return Ok(datPhongs);
        }

        // GET: api/DatPhong/phong/P001
        [HttpGet("GetByMaPhong")]
        public async Task<IActionResult> GetByMaPhong(string maPhong)
        {
            var datPhongs = await _datPhong.GetByMaPhongAsync(maPhong);
            return Ok(datPhongs);
        }

        // GET: api/DatPhong/trangthai/DaNhanPhong
        [HttpGet("GetByTrangThai")]
        public async Task<IActionResult> GetByTrangThai(string trangThai)
        {
            var datPhongs = await _datPhong.GetByTrangThaiAsync(trangThai);
            return Ok(datPhongs);
        }
    }
}