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
        List<PhongMD> GetByTrangThai(string trangThai);
        JsonResult UpdateTrangThai(string maPhong, string trangThai);
        List<PhongMD> GetByLoaiPhong(int maLoaiPhong);
        Dictionary<string, int> GetRoomStatusStatistics();
        bool IsRoomAvailable(string maPhong, DateTime startDate, DateTime endDate);
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
            var phong = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation) // THAY ĐỔI: Thêm Include để lấy thông tin từ LoaiPhong
                .Select(p => new PhongMD
                {
                    MaPhong = p.MaPhong,
                    MaLoaiPhong = p.MaLoaiPhong,
                    TenPhong = p.TenPhong,
                    TrangThai = p.TrangThai,
                    TenLoaiPhong = p.MaLoaiPhongNavigation.TenLoaiPhong, // THAY ĐỔI: Thêm TenLoaiPhong
                    GiaCoBan = p.MaLoaiPhongNavigation.GiaCoBan,         // THAY ĐỔI: Thêm GiaCoBan
                    SoNguoiToiDa = p.MaLoaiPhongNavigation.SoNguoiToiDa // THAY ĐỔI: Thêm SoNguoiToiDa
                })
                .ToList();
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
            var phong = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation) // THAY ĐỔI: Thêm Include để lấy thông tin từ LoaiPhong
                .FirstOrDefault(u => u.MaPhong == MaPhong);

            if (phong == null)
            {
                return new JsonResult("Không tìm thấy phòng")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else
            {
                var _phong = new PhongMD
                {
                    MaPhong = phong.MaPhong, // THAY ĐỔI: Thêm MaPhong vào kết quả
                    MaLoaiPhong = phong.MaLoaiPhong,
                    TenPhong = phong.TenPhong,
                    TrangThai = phong.TrangThai,
                    TenLoaiPhong = phong.MaLoaiPhongNavigation.TenLoaiPhong, // THAY ĐỔI: Thêm TenLoaiPhong
                    GiaCoBan = phong.MaLoaiPhongNavigation.GiaCoBan,         // THAY ĐỔI: Thêm GiaCoBan
                    SoNguoiToiDa = phong.MaLoaiPhongNavigation.SoNguoiToiDa // THAY ĐỔI: Thêm SoNguoiToiDa
                };
                return new JsonResult(_phong);
            }
        }

        public List<PhongMD> GetByTrangThai(string trangThai)
        {
            var phongList = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation) // THAY ĐỔI: Thêm Include để lấy thông tin từ LoaiPhong
                .Where(p => p.TrangThai == trangThai)
                .Select(p => new PhongMD
                {
                    MaPhong = p.MaPhong,
                    MaLoaiPhong = p.MaLoaiPhong,
                    TenPhong = p.TenPhong,
                    TrangThai = p.TrangThai,
                    TenLoaiPhong = p.MaLoaiPhongNavigation.TenLoaiPhong, // THAY ĐỔI: Thêm TenLoaiPhong
                    GiaCoBan = p.MaLoaiPhongNavigation.GiaCoBan,         // THAY ĐỔI: Thêm GiaCoBan
                    SoNguoiToiDa = p.MaLoaiPhongNavigation.SoNguoiToiDa // THAY ĐỔI: Thêm SoNguoiToiDa
                })
                .ToList();

            return phongList;
        }

        public JsonResult UpdateTrangThai(string maPhong, string trangThai)
        {
            var phong = _context.Phongs.SingleOrDefault(p => p.MaPhong == maPhong);
            if (phong == null)
            {
                return new JsonResult("Không tìm thầy phòng cần cập nhật trạng thái")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            phong.TrangThai = trangThai;
            _context.SaveChanges();
            return new JsonResult("Cập nhật trạng thái phòng thành công")
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        public List<PhongMD> GetByLoaiPhong(int maLoaiPhong)
        {
            var phongList = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation) // THAY ĐỔI: Thêm Include để lấy thông tin từ LoaiPhong
                .Where(p => p.MaLoaiPhong == maLoaiPhong)
                .Select(p => new PhongMD
                {
                    MaPhong = p.MaPhong,
                    MaLoaiPhong = p.MaLoaiPhong,
                    TenPhong = p.TenPhong,
                    TrangThai = p.TrangThai,
                    TenLoaiPhong = p.MaLoaiPhongNavigation.TenLoaiPhong, // THAY ĐỔI: Thêm TenLoaiPhong
                    GiaCoBan = p.MaLoaiPhongNavigation.GiaCoBan,         // THAY ĐỔI: Thêm GiaCoBan
                    SoNguoiToiDa = p.MaLoaiPhongNavigation.SoNguoiToiDa // THAY ĐỔI: Thêm SoNguoiToiDa
                })
                .ToList();

            return phongList;
        }

        public Dictionary<string, int> GetRoomStatusStatistics()
        {
            var statistics = _context.Phongs
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                .ToDictionary(x => x.TrangThai, x => x.Count);

            return statistics;
        }

        public bool IsRoomAvailable(string maPhong, DateTime startDate, DateTime endDate)
        {
            var start = startDate;
            var end = endDate;

            var conflictingBookings = _context.DatPhongs
                .Where(dp => dp.MaPhong == maPhong &&
                             (dp.TrangThai == "Đang sử dụng" || dp.TrangThai == "Đã đặt" || dp.TrangThai == "Bảo trì") &&
                             ((dp.NgayNhanPhong <= end && dp.NgayTraPhong >= start) ||
                              (dp.NgayNhanPhong >= start && dp.NgayTraPhong <= end)))
                .Any();

            return !conflictingBookings;
        }
    }
}