

-- Bảng Vai trò (Role)
CREATE TABLE VaiTro (
    MaVaiTro INT PRIMARY KEY IDENTITY(1,1),
    TenVaiTro NVARCHAR(50) NOT NULL -- Ví dụ: 'Nhân viên', 'Quản lý'
);
GO

-- Bảng Nhân viên
CREATE TABLE NhanVien (
    MaNV INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARBINARY(64) NOT NULL, -- Nên mã hóa mật khẩu trong thực tế
    MaVaiTro INT,
    SoDienThoai VARCHAR(15),
    Email VARCHAR(50),
    GioiTinh NVARCHAR(10),
    DiaChi NVARCHAR(100),
    NgaySinh DATE,
    IsActive BIT NOT NULL DEFAULT 1, -- Thêm cột IsActive
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- Bảng Khách hàng
CREATE TABLE KhachHang (
    MaKH INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    CCCD_Passport VARCHAR(20) UNIQUE,
    SoDienThoai VARCHAR(15),
    QuocTich NVARCHAR(50),
    GhiChu NVARCHAR(200)
);
GO

-- Bảng Loại phòng
CREATE TABLE LoaiPhong (
    MaLoaiPhong INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiPhong NVARCHAR(50) NOT NULL,
    GiaCoBan DECIMAL(12,2) NOT NULL,
    SoNguoiToiDa INT NOT NULL
);
GO

-- Bảng Phụ thu (tách riêng từ LoaiPhong)
CREATE TABLE PhuThu (
    MaPhuThu INT PRIMARY KEY IDENTITY(1,1),
    MaLoaiPhong INT,
    PhuThuNguoiThem DECIMAL(12,2) DEFAULT 200000, -- Phụ thu cho mỗi người vượt quá sức chứa
    FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhong(MaLoaiPhong)
);
GO

-- Bảng Phòng
CREATE TABLE Phong (
    MaPhong VARCHAR(10) PRIMARY KEY,
    MaLoaiPhong INT,
    TenPhong NVARCHAR(50),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang sử dụng', N'Bảo trì')),
    FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhong(MaLoaiPhong)
);
GO

-- Bảng Đặt phòng (hỗ trợ một khách đặt nhiều phòng)
CREATE TABLE DatPhong (
    MaDatPhong INT PRIMARY KEY IDENTITY(1,1),
    MaNV INT,
    MaKH INT,
    MaPhong VARCHAR(10),
    NgayDat DATE DEFAULT GETDATE(),
    NgayNhanPhong DATE NOT NULL,
    NgayTraPhong DATE NOT NULL,
    SoNguoiO INT NOT NULL,
    PhuThu DECIMAL(12,2) DEFAULT 0,
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Chờ xác nhận', N'Đã xác nhận', N'Hủy', N'Hoàn thành')),
    TongTienPhong DECIMAL(12,2),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong)
);
GO

-- Bảng Dịch vụ
CREATE TABLE DichVu (
    MaDichVu INT PRIMARY KEY IDENTITY(1,1),
    TenDichVu NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(12,2) NOT NULL,
    MoTa NVARCHAR(200)
);
GO

-- Bảng Sử dụng dịch vụ (hỗ trợ một khách dùng nhiều dịch vụ)
CREATE TABLE SuDungDichVu (
    MaSuDung INT PRIMARY KEY IDENTITY(1,1),
    MaDatPhong INT,
    MaDichVu INT,
    SoLuong INT NOT NULL,
    NgaySuDung DATE DEFAULT GETDATE(),
    NgayKetThuc DATE NULL, -- Thêm cột NgayKetThuc
    ThanhTien DECIMAL(12,2),
    FOREIGN KEY (MaDatPhong) REFERENCES DatPhong(MaDatPhong),
    FOREIGN KEY (MaDichVu) REFERENCES DichVu(MaDichVu)
);
GO

-- Bảng Hóa đơn
CREATE TABLE HoaDon (
    MaHoaDon INT PRIMARY KEY IDENTITY(1,1),
    MaKH INT,
    MaNV INT,
    NgayLap DATE DEFAULT GETDATE(),
    TongTien DECIMAL(12,2),
    PhuongThucThanhToan NVARCHAR(50),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Chưa thanh toán', N'Đã thanh toán')),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);
GO

-- Bảng Chi tiết hóa đơn (liên kết giữa HoaDon và DatPhong)
CREATE TABLE ChiTietHoaDon (
    MaChiTietHoaDon INT PRIMARY KEY IDENTITY(1,1),
    MaHoaDon INT,
    MaDatPhong INT,
    TongTienPhong DECIMAL(12,2), -- Tổng tiền phòng (bao gồm phụ thu)
    TongTienDichVu DECIMAL(12,2), -- Tổng tiền dịch vụ cho đặt phòng này
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    FOREIGN KEY (MaDatPhong) REFERENCES DatPhong(MaDatPhong)
);
GO

-- Thêm dữ liệu mẫu
-- VaiTro
INSERT INTO VaiTro (TenVaiTro)
VALUES 
    (N'Nhân viên'),
    (N'Quản lý');
GO

-- NhanVien
INSERT INTO NhanVien (HoTen, TenDangNhap, MatKhau, MaVaiTro, SoDienThoai, Email, GioiTinh, DiaChi, NgaySinh, IsActive)
VALUES 
    (N'Nguyễn Văn A', 'nva', CONVERT(VARBINARY(64), HASHBYTES('SHA2_512', 'password123')), 1, '0909123456', 'nva@example.com', N'Nam', N'123 Đường ABC, Quận XYZ, TP.HCM', '2000-01-01', 1),
    (N'Trần Thị B', 'ttb', CONVERT(VARBINARY(64), HASHBYTES('SHA2_512', 'password456')), 2, '0909123457', 'ttb@example.com', N'Nữ', N'456 Đường DEF, Quận UVW, TP.HCM', '1995-05-15', 1);
GO

-- LoaiPhong
INSERT INTO LoaiPhong (TenLoaiPhong, GiaCoBan, SoNguoiToiDa)
VALUES 
    (N'Phòng Đơn', 500000, 1),
    (N'Phòng Đôi', 800000, 2);
GO

-- PhuThu
INSERT INTO PhuThu (MaLoaiPhong, PhuThuNguoiThem)
VALUES 
    (1, 200000),
    (2, 250000);
GO

-- Phong
INSERT INTO Phong (MaPhong, MaLoaiPhong, TenPhong, TrangThai)
VALUES 
    ('P101', 1, N'Phòng 101', N'Trống'),
    ('P102', 2, N'Phòng 102', N'Trống'),
    ('P103', 2, N'Phòng 103', N'Trống');
GO

-- DichVu
INSERT INTO DichVu (TenDichVu, DonGia, MoTa)
VALUES 
    (N'Ăn sáng', 100000, N'Bữa sáng buffet'),
    (N'Giặt là', 50000, N'Giặt và ủi quần áo'),
    (N'Massage', 300000, N'Massage thư giãn 60 phút');
GO

-- KhachHang
INSERT INTO KhachHang (HoTen, CCCD_Passport, SoDienThoai, QuocTich, GhiChu)
VALUES 
    (N'Lê Văn C', '123456789', '0935123456', N'Việt Nam', N'Khách hàng thân thiết'),
    (N'Nguyễn Thị D', '987654321', '0935123457', N'Việt Nam', NULL);
GO

-- DatPhong
INSERT INTO DatPhong (MaNV, MaKH, MaPhong, NgayNhanPhong, NgayTraPhong, SoNguoiO, TrangThai, TongTienPhong)
VALUES 
    (1, 1, 'P101', '2025-04-01', '2025-04-03', 1, N'Đã xác nhận', 1000000), -- Phòng đơn, 2 ngày
    (1, 1, 'P102', '2025-04-01', '2025-04-03', 3, N'Đã xác nhận', 1850000), -- Phòng đôi, 2 ngày, vượt 1 người (phụ thu 250000)
    (1, 2, 'P103', '2025-04-01', '2025-04-03', 2, N'Đã xác nhận', 1600000); -- Phòng đôi, 2 ngày
GO

-- SuDungDichVu
INSERT INTO SuDungDichVu (MaDatPhong, MaDichVu, SoLuong, NgaySuDung, NgayKetThuc, ThanhTien)
VALUES 
    (1, 1, 2, '2025-04-01', '2025-04-01', 200000), -- 2 phần ăn sáng cho P101
    (2, 2, 1, '2025-04-01', '2025-04-02', 50000),  -- 1 lần giặt là cho P102
    (3, 3, 1, '2025-04-01', '2025-04-01', 300000); -- 1 lần massage cho P103
GO

-- HoaDon
INSERT INTO HoaDon (MaKH, MaNV, TongTien, PhuongThucThanhToan, TrangThai)
VALUES 
    (1, 1, 0, N'Tiền mặt', N'Chưa thanh toán'),
    (2, 1, 0, N'Thẻ tín dụng', N'Chưa thanh toán');
GO

-- ChiTietHoaDon
INSERT INTO ChiTietHoaDon (MaHoaDon, MaDatPhong, TongTienPhong, TongTienDichVu)
VALUES 
    (1, 1, 1000000, 200000), -- Hóa đơn 1: P101 (1 triệu) + dịch vụ (200 nghìn)
    (1, 2, 1850000, 50000),  -- Hóa đơn 1: P102 (1.85 triệu) + dịch vụ (50 nghìn)
    (2, 3, 1600000, 300000); -- Hóa đơn 2: P103 (1.6 triệu) + dịch vụ (300 nghìn)
GO

-- Cập nhật tổng tiền hóa đơn
UPDATE HoaDon
SET TongTien = (
    SELECT SUM(cthd.TongTienPhong + cthd.TongTienDichVu)
    FROM ChiTietHoaDon cthd
    WHERE cthd.MaHoaDon = HoaDon.MaHoaDon
)
WHERE MaHoaDon IN (1, 2);
GO

-- Truy vấn kiểm tra dữ liệu
SELECT 
    hd.MaHoaDon,
    kh.HoTen AS TenKhachHang,
    p.TenPhong,
    dp.NgayNhanPhong,
    dp.NgayTraPhong,
    dp.SoNguoiO,
    cthd.TongTienPhong,
    dv.TenDichVu,
    sddv.SoLuong,
    sddv.NgaySuDung,
    sddv.NgayKetThuc,
    sddv.ThanhTien,
    cthd.TongTienDichVu,
    hd.TongTien AS TongTienHoaDon
FROM HoaDon hd
JOIN KhachHang kh ON hd.MaKH = kh.MaKH
JOIN ChiTietHoaDon cthd ON hd.MaHoaDon = cthd.MaHoaDon
JOIN DatPhong dp ON cthd.MaDatPhong = dp.MaDatPhong
JOIN Phong p ON dp.MaPhong = p.MaPhong
LEFT JOIN SuDungDichVu sddv ON dp.MaDatPhong = sddv.MaDatPhong
LEFT JOIN DichVu dv ON sddv.MaDichVu = dv.MaDichVu
WHERE hd.MaHoaDon = 1
AND dp.TrangThai NOT IN (N'Hủy');
GO
