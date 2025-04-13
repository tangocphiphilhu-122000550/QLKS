using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKS.Repository
{
    public interface ILoaiPhongRepository
    {
        List<LoaiPhongMD> GetAll();
        JsonResult GetById(int maLoaiPhong);
        JsonResult AddLoaiPhong(LoaiPhongVM loaiPhongVM);
        JsonResult EditLoaiPhong(int maLoaiPhong, LoaiPhongVM loaiPhongVM);
        JsonResult DeleteLoaiPhong(int maLoaiPhong);
    }

    public class LoaiPhongRepository : ILoaiPhongRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public LoaiPhongRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public List<LoaiPhongMD> GetAll()
        {
            var loaiPhongs = _context.LoaiPhongs
                .Select(lp => new LoaiPhongMD
                {
                    MaLoaiPhong = lp.MaLoaiPhong,
                    TenLoaiPhong = lp.TenLoaiPhong,
                    GiaCoBan = lp.GiaCoBan,
                    SoNguoiToiDa = lp.SoNguoiToiDa
                })
                .ToList();

            return loaiPhongs;
        }

        public JsonResult GetById(int maLoaiPhong)
        {
            var loaiPhong = _context.LoaiPhongs
                .FirstOrDefault(lp => lp.MaLoaiPhong == maLoaiPhong);

            if (loaiPhong == null)
            {
                return new JsonResult("Không tìm thấy loại phòng")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var loaiPhongMD = new LoaiPhongMD
            {
                MaLoaiPhong = loaiPhong.MaLoaiPhong,
                TenLoaiPhong = loaiPhong.TenLoaiPhong,
                GiaCoBan = loaiPhong.GiaCoBan,
                SoNguoiToiDa = loaiPhong.SoNguoiToiDa
            };

            return new JsonResult(loaiPhongMD);
        }

        public JsonResult AddLoaiPhong(LoaiPhongVM loaiPhongVM)
        {
            // Kiểm tra trùng TenLoaiPhong
            var check = _context.LoaiPhongs
                .FirstOrDefault(lp => lp.TenLoaiPhong == loaiPhongVM.TenLoaiPhong);
            if (check != null)
            {
                return new JsonResult("Loại phòng đã tồn tại")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var loaiPhong = new LoaiPhong
            {
                TenLoaiPhong = loaiPhongVM.TenLoaiPhong,
                GiaCoBan = loaiPhongVM.GiaCoBan,
                SoNguoiToiDa = loaiPhongVM.SoNguoiToiDa
            };

            _context.LoaiPhongs.Add(loaiPhong);
            _context.SaveChanges();

            return new JsonResult("Đã thêm loại phòng")
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        public JsonResult EditLoaiPhong(int maLoaiPhong, LoaiPhongVM loaiPhongVM)
        {
            var loaiPhong = _context.LoaiPhongs
                .SingleOrDefault(lp => lp.MaLoaiPhong == maLoaiPhong);

            if (loaiPhong == null)
            {
                return new JsonResult("Không tìm thấy loại phòng cần sửa")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Kiểm tra trùng TenLoaiPhong với loại phòng khác
            var checkDuplicate = _context.LoaiPhongs
                .FirstOrDefault(lp => lp.TenLoaiPhong == loaiPhongVM.TenLoaiPhong && lp.MaLoaiPhong != maLoaiPhong);
            if (checkDuplicate != null)
            {
                return new JsonResult("Tên loại phòng đã tồn tại")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            loaiPhong.TenLoaiPhong = loaiPhongVM.TenLoaiPhong;
            loaiPhong.GiaCoBan = loaiPhongVM.GiaCoBan;
            loaiPhong.SoNguoiToiDa = loaiPhongVM.SoNguoiToiDa;

            _context.SaveChanges();

            return new JsonResult("Đã chỉnh sửa loại phòng")
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        public JsonResult DeleteLoaiPhong(int maLoaiPhong)
        {
            var loaiPhong = _context.LoaiPhongs
                .SingleOrDefault(lp => lp.MaLoaiPhong == maLoaiPhong);

            if (loaiPhong == null)
            {
                return new JsonResult("Không tìm thấy loại phòng cần xóa")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Kiểm tra xem loại phòng có đang được sử dụng bởi phòng nào không
            var phongUsingLoaiPhong = _context.Phongs
                .Any(p => p.MaLoaiPhong == maLoaiPhong);
            if (phongUsingLoaiPhong)
            {
                return new JsonResult("Không thể xóa loại phòng vì đang được sử dụng bởi ít nhất một phòng")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            _context.LoaiPhongs.Remove(loaiPhong);
            _context.SaveChanges();

            return new JsonResult("Đã xóa loại phòng")
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}