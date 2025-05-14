using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKS.Models;
using QLKS.Repository;
using QLKS.Helpers;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace QLKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonRepository _hoaDonRepository;
        private readonly EmailHelper _emailHelper;

        public HoaDonController(IHoaDonRepository hoaDonRepository, EmailHelper emailHelper)
        {
            _hoaDonRepository = hoaDonRepository;
            _emailHelper = emailHelper;
        }

        [Authorize(Roles = "NhanVien")]
        [HttpGet]
        public async Task<ActionResult<PagedHoaDonResponse>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _hoaDonRepository.GetAllAsync(pageNumber, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpGet("TenKhachHang/{tenKhachHang}")]
        public async Task<ActionResult<PagedHoaDonResponse>> GetByTenKhachHang(string tenKhachHang, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _hoaDonRepository.GetByTenKhachHangAsync(tenKhachHang, pageNumber, pageSize);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPost("Create")]
        public async Task<ActionResult<HoaDonVM>> Create([FromBody] CreateHoaDonVM hoaDonVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newHoaDon = await _hoaDonRepository.CreateAsync(hoaDonVM);
                return CreatedAtAction(nameof(GetAll), new { maHoaDon = newHoaDon.MaHoaDon }, newHoaDon);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPost("ExportPdf")]
        public async Task<IActionResult> ExportPdf([FromBody] ExportHoaDonRequest request)
        {
            if (request == null || request.MaHoaDon <= 0)
            {
                return BadRequest("MaHoaDon là trường bắt buộc và phải lớn hơn 0.");
            }

            try
            {
                var pdfData = await _hoaDonRepository.ExportInvoicePdfAsync(request.MaHoaDon);
                var fileName = $"HoaDon_{request.MaHoaDon}.pdf";

                // Trả về file PDF để tải về máy
                Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
                return File(pdfData, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPost("ExportPdfWithEmail")]
        public async Task<IActionResult> ExportPdfWithEmail([FromBody] ExportHoaDonWithEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(request.Email, emailRegex))
                    return BadRequest("Email không hợp lệ.");

                var pdfData = await _hoaDonRepository.ExportInvoicePdfAsync(request.MaHoaDon);
                var fileName = $"HoaDon_{request.MaHoaDon}.pdf";

                var subject = $"Hóa Đơn Thanh Toán - Mã {request.MaHoaDon}";
                var body = $"Kính gửi Quý Khách,\n\nĐính kèm là hóa đơn thanh toán (Mã: {request.MaHoaDon}) từ Khách Sạn Hoàng Gia.\nVui lòng xem chi tiết về phòng và các dịch vụ đã sử dụng trong file PDF đính kèm.\nCảm ơn Quý Khách đã sử dụng dịch vụ của chúng tôi!\n\nTrân trọng,\nKhách Sạn Hoàng Gia";
                await _emailHelper.SendEmailAsync(request.Email, subject, body, isHtml: false, attachmentData: pdfData, attachmentName: fileName);
                return Ok(new { Message = $"Hóa đơn đã được gửi đến {request.Email}." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPut("UpdateTrangThaiByTenKhachHang/{tenKhachHang}")]
        public async Task<IActionResult> UpdateTrangThaiByTenKhachHang(string tenKhachHang, [FromBody] UpdateHoaDonVM updateVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hoaDonRepository.UpdateTrangThaiByTenKhachHangAsync(tenKhachHang, updateVM);
                if (!result)
                    return NotFound($"Không tìm thấy hóa đơn nào cho tên khách hàng: {tenKhachHang}");

                return Ok("Cập nhật trạng thái thành công.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [Authorize(Roles = "NhanVien")]
        [HttpPut("UpdatePhuongThucThanhToanByTenKhachHang/{tenKhachHang}")]
        public async Task<IActionResult> UpdatePhuongThucThanhToanByTenKhachHang(string tenKhachHang, [FromBody] UpdatePhuongThucThanhToanVM updateVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hoaDonRepository.UpdatePhuongThucThanhToanByTenKhachHangAsync(tenKhachHang, updateVM);
                if (!result)
                    return NotFound($"Không tìm thấy hóa đơn nào cho tên khách hàng: {tenKhachHang}");

                return Ok("Cập nhật phương thức thanh toán thành công.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }
    }
}