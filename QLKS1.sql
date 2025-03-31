-- Bảng Vai trò (Role) - bỏ cột MoTa
CREATE TABLE VaiTro (
    MaVaiTro INT PRIMARY KEY IDENTITY(1,1),
    TenVaiTro NVARCHAR(50) NOT NULL -- Ví dụ: 'Nhân viên', 'Quản lý'
);

-- Bảng Nhân viên
CREATE TABLE NhanVien (
    MaNV INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau varbinary(64) NOT NULL, -- Nên mã hóa mật khẩu trong thực tế
    MaVaiTro INT,
    SoDienThoai VARCHAR(15),
    Email VARCHAR(50),
	GioiTinh nVARCHAR(10),
	DiaChi nvarchar(100),
	NgaySinh date
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);

-- Bảng Khách hàng
CREATE TABLE KhachHang (
    MaKH INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    CCCD_Passport VARCHAR(20) UNIQUE,
    SoDienThoai VARCHAR(15),
    QuocTich NVARCHAR(50),
    GhiChu NVARCHAR(200)
);

-- Bảng Loại phòng
CREATE TABLE LoaiPhong (
    MaLoaiPhong INT PRIMARY KEY IDENTITY(1,1),
    TenLoaiPhong NVARCHAR(50) NOT NULL,
    GiaCoBan DECIMAL(12,2) NOT NULL,
    SoNguoiToiDa INT NOT NULL
);

-- Bảng Phụ thu (tách riêng từ LoaiPhong)
CREATE TABLE PhuThu (
    MaPhuThu INT PRIMARY KEY IDENTITY(1,1),
    MaLoaiPhong INT,
    PhuThuNguoiThem DECIMAL(12,2) DEFAULT 200000, -- Phụ thu cho mỗi người vượt quá sức chứa
    FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhong(MaLoaiPhong)
);

-- Bảng Phòng
CREATE TABLE Phong (
    MaPhong VARCHAR(10) PRIMARY KEY,
    MaLoaiPhong INT,
    TenPhong NVARCHAR(50),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang sử dụng', N'Bảo trì')),
    FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhong(MaLoaiPhong)
);

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

-- Bảng Dịch vụ (bỏ cột DonViTinh)
CREATE TABLE DichVu (
    MaDichVu INT PRIMARY KEY IDENTITY(1,1),
    TenDichVu NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(12,2) NOT NULL,
    MoTa NVARCHAR(200)
);

-- Bảng Sử dụng dịch vụ (hỗ trợ một khách dùng nhiều dịch vụ)
CREATE TABLE SuDungDichVu (
    MaSuDung INT PRIMARY KEY IDENTITY(1,1),
    MaDatPhong INT,
    MaDichVu INT,
    SoLuong INT NOT NULL,
    NgaySuDung DATE DEFAULT GETDATE(),
    ThanhTien DECIMAL(12,2),
    FOREIGN KEY (MaDatPhong) REFERENCES DatPhong(MaDatPhong),
    FOREIGN KEY (MaDichVu) REFERENCES DichVu(MaDichVu)
);

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



-- Thêm dữ liệu mẫu
INSERT INTO VaiTro (TenVaiTro)
VALUES 
    (N'Nhân viên'),
    (N'Quản lý');

INSERT INTO NhanVien (HoTen, TenDangNhap, MatKhau, MaVaiTro, SoDienThoai, Email, GioiTinh, DiaChi, NgaySinh)
VALUES (
    N'Nguyễn Văn A', -- HoTen
    'nva', -- TenDangNhap
    CONVERT(VARBINARY(64), HASHBYTES('SHA2_512', 'password123')), -- MatKhau (đã mã hóa)
    1, -- MaVaiTro
    '0909123456', -- SoDienThoai
    'nva@example.com', -- Email (ví dụ)
    N'Nam', -- GioiTinh (ví dụ)
    N'123 Đường ABC, Quận XYZ, TP.HCM', -- DiaChi (ví dụ)
    '2000-01-01' -- NgaySinh (ví dụ)
);
INSERT INTO LoaiPhong (TenLoaiPhong, GiaCoBan, SoNguoiToiDa)
VALUES 
    (N'Phòng Đơn', 500000, 1),
    (N'Phòng Đôi', 800000, 2);

INSERT INTO PhuThu (MaLoaiPhong, PhuThuNguoiThem)
VALUES 
    (1, 200000),
    (2, 250000);

INSERT INTO Phong (MaPhong, MaLoaiPhong, TenPhong, TrangThai)
VALUES 
    ('P101', 1, N'Phòng 101', N'Trống'),
    ('P102', 2, N'Phòng 102', N'Trống'),
    ('P103', 2, N'Phòng 103', N'Trống');

INSERT INTO DichVu (TenDichVu, DonGia)
VALUES 
    (N'Ăn sáng', 100000),
    (N'Giặt là', 50000),
    (N'Massage', 300000);

-- Ví dụ: Khách hàng đặt 3 phòng và 3 dịch vụ
INSERT INTO KhachHang (HoTen, CCCD_Passport, SoDienThoai)
VALUES (N'Lê Văn C', '123456789', '0935123456');

INSERT INTO DatPhong (MaNV, MaKH, MaPhong, NgayNhanPhong, NgayTraPhong, SoNguoiO, TrangThai)
VALUES 
    (1, 1, 'P101', '2025-04-01', '2025-04-03', 1, N'Đã xác nhận'), -- Phòng đơn
    (1, 1, 'P102', '2025-04-01', '2025-04-03', 3, N'Đã xác nhận'), -- Phòng đôi, vượt 1 người
    (1, 1, 'P103', '2025-04-01', '2025-04-03', 2, N'Đã xác nhận'); -- Phòng đôi

INSERT INTO SuDungDichVu (MaDatPhong, MaDichVu, SoLuong)
VALUES 
    (1, 1, 2), -- 2 phần ăn sáng cho P101
    (2, 2, 1), -- 1 lần giặt là cho P102
    (3, 3, 1); -- 1 lần massage cho P103

INSERT INTO HoaDon (MaKH, MaNV, TongTien, PhuongThucThanhToan, TrangThai)
VALUES (1, 1, 0, N'Tiền mặt', N'Chưa thanh toán');

-- Cập nhật tổng tiền hóa đơn
UPDATE HoaDon
SET TongTien = (
    SELECT SUM(dp.TongTienPhong) + SUM(sddv.ThanhTien)
    FROM DatPhong dp
    LEFT JOIN SuDungDichVu sddv ON dp.MaDatPhong = sddv.MaDatPhong
    WHERE dp.MaKH = HoaDon.MaKH
)
WHERE MaHoaDon = 1;

SELECT 
    p.TenPhong,
    dv.TenDichVu,
    sddv.SoLuong,
    dv.DonGia,
    sddv.ThanhTien,
    sddv.NgaySuDung
FROM Phong p
JOIN DatPhong dp ON p.MaPhong = dp.MaPhong
JOIN SuDungDichVu sddv ON dp.MaDatPhong = sddv.MaDatPhong
JOIN DichVu dv ON sddv.MaDichVu = dv.MaDichVu
WHERE p.MaPhong = 'P102'
AND dp.TrangThai NOT IN (N'Hủy');


Alter table NhanVien
Add IsActive Bit not null default 1