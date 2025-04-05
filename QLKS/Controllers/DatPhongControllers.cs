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
        private readonly IDatPhongRepository _datPhong;

        public DatPhongController(IDatPhongRepository datPhong)
        {
            _datPhong = datPhong;
        }

        // GET: api/DatPhong/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetAll()
        {
            try
            {
                var datPhongEntities = await _datPhong.GetAllAsync();

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
                    SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0,
                    DanhSachDichVu = dp.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                    {
                        MaSuDung = sddv.MaSuDung,
                        MaDichVu = sddv.MaDichVu,
                        TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                        SoLuong = sddv.SoLuong,
                        NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        ThanhTien = sddv.ThanhTien
                    }).ToList() ?? new List<SuDungDichVuVM>()
                });

                return Ok(datPhongVMs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi lấy danh sách đặt phòng: " + ex.Message });
            }
        }

        // GET: api/DatPhong/GetById/5
        [HttpGet("GetByMaDatPhong/{id}")]
        public async Task<ActionResult<DatPhongVM>> GetById(int id)
        {
            try
            {
                var dp = await _datPhong.GetByIdAsync(id);
                if (dp == null)
                {
                    return NotFound(new { Message = "Đặt phòng không tồn tại." });
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
                    SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0,
                    DanhSachDichVu = dp.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                    {
                        MaSuDung = sddv.MaSuDung,
                        MaDichVu = sddv.MaDichVu,
                        TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                        SoLuong = sddv.SoLuong,
                        NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        ThanhTien = sddv.ThanhTien
                    }).ToList() ?? new List<SuDungDichVuVM>()
                };

                return Ok(datPhongVM);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi lấy thông tin đặt phòng: " + ex.Message });
            }
        }

        // GET: api/DatPhong/GetByMaPhong
        [HttpGet("GetByMaPhong/{maPhong}")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByMaPhong(string maPhong)
        {
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
                    SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0,
                    DanhSachDichVu = dp.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                    {
                        MaSuDung = sddv.MaSuDung,
                        MaDichVu = sddv.MaDichVu,
                        TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                        SoLuong = sddv.SoLuong,
                        NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        ThanhTien = sddv.ThanhTien
                    }).ToList() ?? new List<SuDungDichVuVM>()
                });

                return Ok(datPhongVMs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi lấy danh sách đặt phòng: " + ex.Message });
            }
        }

        // POST: api/DatPhong/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DatPhongVM datPhongVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var datPhong = new DatPhong
                {
                    MaNv = datPhongVM.MaNv,
                    MaKh = datPhongVM.MaKh,
                    MaPhong = datPhongVM.MaPhong,
                    NgayDat = datPhongVM.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                    NgayNhanPhong = datPhongVM.NgayNhanPhong,
                    NgayTraPhong = datPhongVM.NgayTraPhong,
                    SoNguoiO = datPhongVM.SoNguoiO,
                    PhuThu = datPhongVM.PhuThu,
                    TrangThai = datPhongVM.TrangThai ?? "Chờ xác nhận",
                    TongTienPhong = datPhongVM.TongTienPhong
                };

                var createdDatPhong = await _datPhong.AddAsync(datPhong);
                var newDp = await _datPhong.GetByIdAsync(createdDatPhong.MaDatPhong);
                if (newDp == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Không thể truy xuất đặt phòng vừa tạo." });
                }

                var newDatPhongVM = new DatPhongVM
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
                    SoLuongDichVuSuDung = newDp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0,
                    DanhSachDichVu = newDp.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                    {
                        MaSuDung = sddv.MaSuDung,
                        MaDichVu = sddv.MaDichVu,
                        TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                        SoLuong = sddv.SoLuong,
                        NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        ThanhTien = sddv.ThanhTien
                    }).ToList() ?? new List<SuDungDichVuVM>()
                };

                return CreatedAtAction(nameof(GetById), new { id = createdDatPhong.MaDatPhong }, newDatPhongVM);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi tạo đặt phòng: " + ex.Message });
            }
        }

        // GET: api/DatPhong/GetByMaKh/5
        [HttpGet("GetByMaKh/{maKh}")]
        public async Task<ActionResult<IEnumerable<DatPhongVM>>> GetByMaKh(int maKh)
        {
            try
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
                    SoLuongDichVuSuDung = dp.SuDungDichVus?.Sum(sddv => sddv.SoLuong) ?? 0,
                    DanhSachDichVu = dp.SuDungDichVus?.Select(sddv => new SuDungDichVuVM
                    {
                        MaSuDung = sddv.MaSuDung,
                        MaDichVu = sddv.MaDichVu,
                        TenDichVu = sddv.MaDichVuNavigation?.TenDichVu,
                        SoLuong = sddv.SoLuong,
                        NgaySuDung = sddv.NgaySuDung.HasValue ? sddv.NgaySuDung.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        NgayKetThuc = sddv.NgayKetThuc.HasValue ? sddv.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        ThanhTien = sddv.ThanhTien
                    }).ToList() ?? new List<SuDungDichVuVM>()
                });

                return Ok(datPhongVMs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi lấy danh sách đặt phòng: " + ex.Message });
            }
        }

        // PUT: api/DatPhong/Update/5
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DatPhongVM datPhongVM)
        {
            try
            {
                if (id != datPhongVM.MaDatPhong)
                {
                    return BadRequest(new { Message = "ID không khớp với dữ liệu." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var datPhong = new DatPhong
                {
                    MaDatPhong = datPhongVM.MaDatPhong,
                    MaNv = datPhongVM.MaNv,
                    MaKh = datPhongVM.MaKh,
                    MaPhong = datPhongVM.MaPhong,
                    NgayDat = datPhongVM.NgayDat ?? DateOnly.FromDateTime(DateTime.Now),
                    NgayNhanPhong = datPhongVM.NgayNhanPhong,
                    NgayTraPhong = datPhongVM.NgayTraPhong,
                    SoNguoiO = datPhongVM.SoNguoiO,
                    PhuThu = datPhongVM.PhuThu,
                    TrangThai = datPhongVM.TrangThai,
                    TongTienPhong = datPhongVM.TongTienPhong
                };

                await _datPhong.UpdateAsync(datPhong);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _datPhong.GetByIdAsync(id) == null)
                {
                    return NotFound(new { Message = "Đặt phòng không tồn tại." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi cập nhật đặt phòng: " + ex.Message });
            }
        }

        // DELETE: api/DatPhong/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _datPhong.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { Message = "Đặt phòng không tồn tại." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi khi xóa đặt phòng: " + ex.Message });
            }
        }
    }
}