using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QLKS.Data;
using QLKS.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QLKS.Models;
using Microsoft.AspNetCore.Mvc;

namespace QLKS.Repository
{
    public interface IPhongRepository
    {
        List<PhongMD> GetAll();
        JsonResult AddPhong(PhongMD phongVM);
        JsonResult EditPhong(string MaPhong, PhongVM phongVM);
        JsonResult DeletePhong(string MaPhong);
        JsonResult GetById(string MaPhong);
    }
    public class PhongRepository : IPhongRepository
    {
        private readonly DataQlks112Nhom3Context _context;

        public PhongRepository(DataQlks112Nhom3Context context)
        {
            _context = context;
        }

        public List<PhongMD> GetAll()
        {
            var phong = _context.Phongs.Select(l => new PhongMD
            {
                MaPhong = l.MaPhong,
                MaLoaiPhong = l.MaLoaiPhong,
                TenPhong = l.TenPhong,
                TrangThai = l.TrangThai,


            }).ToList();
            return phong;
        }

        public JsonResult AddPhong(PhongMD phongVM)
        {
            var check = _context.Phongs.FirstOrDefault(c => c.TenPhong == phongVM.TenPhong || c.MaPhong == phongVM.MaPhong);
            if (check != null)
            {
                return new JsonResult("Phong đã tồn tại")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            else
            {
                var phong = new Phong
                {
                    MaPhong = phongVM.MaPhong,
                    MaLoaiPhong = phongVM.MaLoaiPhong,
                    TenPhong = phongVM.TenPhong,
                    TrangThai = phongVM.TrangThai,
                };
                _context.Phongs.Add(phong);
                _context.SaveChanges();
                return new JsonResult("Đã thêm Phong")
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
        }
        public JsonResult EditPhong(string MaPhong, PhongVM phongVM)
        {
            var phong = _context.Phongs.SingleOrDefault(l => l.MaPhong == MaPhong);
            if (phong == null)
            {
                return new JsonResult("Không tìm thấy dịch vụ cần sửa")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else
            {
                phong.MaLoaiPhong = phongVM.MaLoaiPhong;
                phong.TenPhong = phongVM.TenPhong;
                phong.TrangThai = phongVM.TrangThai;
                _context.SaveChanges();
                return new JsonResult("Đã chỉnh sửa")
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }
        public JsonResult DeletePhong(string MaPhong)
        {
            var phong = _context.Phongs.SingleOrDefault(l => l.MaPhong == MaPhong);
            if (phong == null)
            {
                return new JsonResult("Không tìm thấy phòng cần xoá")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else
            {
                _context.Phongs.Remove(phong);
                _context.SaveChanges();
                return new JsonResult("Đã xoá")
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }
        public JsonResult GetById(string MaPhong)
        {
            var phong = _context.Phongs.FirstOrDefault(u => u.MaPhong == MaPhong);

            if (phong == null)
            {
                return new JsonResult("Không tìm thấy phòng")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else
            {
                var _phong = new PhongVM
                {
                    MaLoaiPhong = phong.MaLoaiPhong,
                    TenPhong = phong.TenPhong,
                    TrangThai = phong.TrangThai,
                };
                return new JsonResult(_phong);
            }
        }
    }
}