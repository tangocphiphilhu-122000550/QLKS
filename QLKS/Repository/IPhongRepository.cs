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
        PagedPhongResponse GetAll(int pageNumber, int pageSize);
        JsonResult AddPhong(PhongMD phongVM);
        JsonResult EditPhong(string MaPhong, PhongVM phongVM);
        JsonResult DeletePhong(string MaPhong);
        JsonResult GetById(string MaPhong);
        PagedPhongResponse GetByTrangThai(string trangThai, int pageNumber, int pageSize);
        JsonResult UpdateTrangThai(string maPhong, string trangThai);
        PagedPhongResponse GetByLoaiPhong(int maLoaiPhong, int pageNumber, int pageSize);
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

        public PagedPhongResponse GetAll(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation);

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var phongs = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PhongMD
                {
                    MaPhong = p.MaPhong,
                    MaLoaiPhong = p.MaLoaiPhong,
                    TenPhong = p.TenPhong,
                    TrangThai = p.TrangThai,
                    TenLoaiPhong = p.MaLoaiPhongNavigation.TenLoaiPhong,
                    GiaCoBan = p.MaLoaiPhongNavigation.GiaCoBan,
                    SoNguoiToiDa = p.MaLoaiPhongNavigation.SoNguoiToiDa
                })
                .ToList();

            return new PagedPhongResponse
            {
                Phongs = phongs,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
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

        public PagedPhongResponse GetByTrangThai(string trangThai, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .Where(p => p.TrangThai == trangThai);

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var phongs = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PhongMD
                {
                    MaPhong = p.MaPhong,
                    MaLoaiPhong = p.MaLoaiPhong,
                    TenPhong = p.TenPhong,
                    TrangThai = p.TrangThai,
                    TenLoaiPhong = p.MaLoaiPhongNavigation.TenLoaiPhong,
                    GiaCoBan = p.MaLoaiPhongNavigation.GiaCoBan,
                    SoNguoiToiDa = p.MaLoaiPhongNavigation.SoNguoiToiDa
                })
                .ToList();

            return new PagedPhongResponse
            {
                Phongs = phongs,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
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

        public PagedPhongResponse GetByLoaiPhong(int maLoaiPhong, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .Where(p => p.MaLoaiPhong == maLoaiPhong);

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var phongs = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PhongMD
                {
                    MaPhong = p.MaPhong,
                    MaLoaiPhong = p.MaLoaiPhong,
                    TenPhong = p.TenPhong,
                    TrangThai = p.TrangThai,
                    TenLoaiPhong = p.MaLoaiPhongNavigation.TenLoaiPhong,
                    GiaCoBan = p.MaLoaiPhongNavigation.GiaCoBan,
                    SoNguoiToiDa = p.MaLoaiPhongNavigation.SoNguoiToiDa
                })
                .ToList();

            return new PagedPhongResponse
            {
                Phongs = phongs,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
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