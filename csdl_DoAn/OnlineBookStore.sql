IF OBJECT_ID('ChiTietGioHang', 'U') IS NOT NULL DROP TABLE ChiTietGioHang;
IF OBJECT_ID('ChiTietDonHang', 'U') IS NOT NULL DROP TABLE ChiTietDonHang;
IF OBJECT_ID('GioHang', 'U') IS NOT NULL DROP TABLE GioHang;
IF OBJECT_ID('DonHang', 'U') IS NOT NULL DROP TABLE DonHang;
IF OBJECT_ID('NguoiDung', 'U') IS NOT NULL DROP TABLE NguoiDung;
IF OBJECT_ID('Admin', 'U') IS NOT NULL DROP TABLE Admin;
IF OBJECT_ID('Sach', 'U') IS NOT NULL DROP TABLE Sach;
IF OBJECT_ID('TheLoai', 'U') IS NOT NULL DROP TABLE TheLoai;
IF OBJECT_ID('NhaXuatBan', 'U') IS NOT NULL DROP TABLE NhaXuatBan;
IF OBJECT_ID('KhuyenMai', 'U') IS NOT NULL DROP TABLE KhuyenMai;
GO


CREATE TABLE TheLoai (
    MaTL INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    TenTheLoai NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE NhaXuatBan (
    MaNXB INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    TenNXB NVARCHAR(150) NOT NULL UNIQUE,
    DiaChi NVARCHAR(255),
    DienThoai VARCHAR(15)
);
GO

CREATE TABLE Sach (
    MaSach INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    TenSach NVARCHAR(255) NOT NULL,
    TacGia NVARCHAR(100) NOT NULL,
    MaTL INT NOT NULL,
    MaNXB INT NOT NULL,
    GiaBan DECIMAL(10, 2) NOT NULL CHECK (GiaBan > 0),
    SoLuongTon INT NOT NULL CHECK (SoLuongTon >= 0),
    MoTa NVARCHAR(MAX),
    HinhAnh VARCHAR(255),
    FOREIGN KEY (MaTL) REFERENCES TheLoai(MaTL),
    FOREIGN KEY (MaNXB) REFERENCES NhaXuatBan(MaNXB)
);
GO

CREATE TABLE NguoiDung (
    MaND INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    DienThoai VARCHAR(15),
    DiaChi NVARCHAR(255),
);
GO

CREATE TABLE Admin (
    MaAdmin INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE KhuyenMai (
    MaKM INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    TenKM NVARCHAR(255) NOT NULL,
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    GiaTriGiam DECIMAL(5, 2) NOT NULL CHECK (GiaTriGiam > 0 AND GiaTriGiam <= 1),
    LoaiApDung NVARCHAR(50)
);
GO

CREATE TABLE DonHang (
    MaDH INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    MaND INT NOT NULL,
    NgayDatHang DATETIME NOT NULL,
    TongTien DECIMAL(10, 2) NOT NULL,
    TrangThaiDH NVARCHAR(50) NOT NULL,
    DiaChiGiaoHang NVARCHAR(255) NOT NULL,
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
GO

CREATE TABLE ChiTietDonHang (
    MaDH INT NOT NULL,
    MaSach INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (MaDH, MaSach),
    FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);
GO

CREATE TABLE GioHang (
    MaGH INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    MaND INT NOT NULL UNIQUE,
    NgayTao DATETIME NOT NULL,
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
GO

CREATE TABLE ChiTietGioHang (
    MaGH INT NOT NULL,
    MaSach INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    PRIMARY KEY (MaGH, MaSach),
    FOREIGN KEY (MaGH) REFERENCES GioHang(MaGH),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);
GO


SET IDENTITY_INSERT TheLoai ON;
INSERT INTO TheLoai (MaTL, TenTheLoai) VALUES (1, N'Tiểu thuyết'), (2, N'Khoa học - Công nghệ'), (3, N'Kinh tế - Khởi nghiệp'), (4, N'Sách thiếu nhi'), (5, N'Tâm lý - Kỹ năng sống');
SET IDENTITY_INSERT TheLoai OFF;
GO

SET IDENTITY_INSERT NhaXuatBan ON;
INSERT INTO NhaXuatBan (MaNXB, TenNXB, DiaChi, DienThoai) VALUES (1, N'Nhà Xuất Bản Trẻ', N'TP. Hồ Chí Minh', '02838227449'), (2, N'Nhà Xuất Bản Kim Đồng', N'Hà Nội', '02439434730'), (3, N'Nhà Xuất Bản Lao Động', N'Hà Nội', '02437345524');
SET IDENTITY_INSERT NhaXuatBan OFF;
GO

SET IDENTITY_INSERT Sach ON;
INSERT INTO Sach (MaSach, TenSach, TacGia, MaTL, MaNXB, GiaBan, SoLuongTon, MoTa) VALUES
(1, N'Nhà Giả Kim', N'Paulo Coelho', 1, 1, 98000.00, 50, N'Một câu chuyện truyền cảm hứng về việc theo đuổi ước mơ.'),
(2, N'Đắc Nhân Tâm', N'Dale Carnegie', 5, 3, 125500.00, 75, N'Nghệ thuật giao tiếp và đối nhân xử thế.'),
(3, N'Lược Sử Thời Gian', N'Stephen Hawking', 2, 3, 150000.00, 30, N'Tác phẩm khoa học nổi tiếng về vũ trụ và thời gian.'),
(4, N'Bí Mật Tư Duy Triệu Phú', N'T. Harv Eker', 3, 1, 105000.00, 60, N'Phân tích sự khác biệt giữa người giàu và người nghèo.');
SET IDENTITY_INSERT Sach OFF;
GO

SET IDENTITY_INSERT NguoiDung ON;
INSERT INTO NguoiDung (MaND, TenDangNhap, MatKhau, HoTen, Email, DienThoai, DiaChi) VALUES
(101, 'khachhang1', 'pass123', N'Nguyễn Văn A', 'khachhang1@gmail.com', '0901234567', N'123 Đường 3/2, Q. Ninh Kiều, Cần Thơ'),
(102, 'khachhang2', 'pass123', N'Trần Thị B', 'khachhang2@gmail.com', '0912345678', N'456 Đường CMT8, Q. Cái Răng, Cần Thơ');
SET IDENTITY_INSERT NguoiDung OFF;
GO

SET IDENTITY_INSERT Admin ON;
INSERT INTO Admin (MaAdmin, TenDangNhap, MatKhau, HoTen) VALUES (1, 'admin_chinh', 'admin123', N'Hồ Quốc Tạo Quản Trị');
SET IDENTITY_INSERT Admin OFF;
GO

SET IDENTITY_INSERT DonHang ON;
INSERT INTO DonHang (MaDH, MaND, NgayDatHang, TongTien, TrangThaiDH, DiaChiGiaoHang) VALUES
(1001, 101, GETDATE(), 373500.00, N'Đã giao thành công', N'123 Đường 3/2, Q. Ninh Kiều, Cần Thơ'),
(1002, 101, DATEADD(DAY, -5, GETDATE()), 150000.00, N'Đang giao hàng', N'123 Đường 3/2, Q. Ninh Kiều, Cần Thơ');
SET IDENTITY_INSERT DonHang OFF;
GO

INSERT INTO ChiTietDonHang (MaDH, MaSach, SoLuong, DonGia) VALUES
(1001, 1, 1, 98000.00), (1001, 2, 2, 125500.00),
(1002, 3, 1, 150000.00);
GO

SET IDENTITY_INSERT GioHang ON;
INSERT INTO GioHang (MaGH, MaND, NgayTao) VALUES (201, 102, GETDATE());
SET IDENTITY_INSERT GioHang OFF;
GO

INSERT INTO ChiTietGioHang (MaGH, MaSach, SoLuong) VALUES
(201, 4, 1), (201, 1, 3);
GO

SET IDENTITY_INSERT KhuyenMai ON;
INSERT INTO KhuyenMai (MaKM, TenKM, NgayBatDau, NgayKetThuc, GiaTriGiam, LoaiApDung) VALUES
(501, N'Giảm 10% cho tất cả đơn hàng', '2025-11-01', '2025-11-30', 0.10, N'Toàn bộ'),
(502, N'Giảm 20% cho sách Tiểu thuyết', '2025-11-20', '2025-12-05', 0.20, N'TheLoai');
SET IDENTITY_INSERT KhuyenMai OFF;
GO
