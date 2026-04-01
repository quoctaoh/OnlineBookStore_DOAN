-- Dán và chạy các lệnh này trong SQL Server Management Studio (SSMS)

-- 1. Cập nhật ảnh cho cuốn 'Nhà Giả Kim' (MaSach = 1)
UPDATE Sach SET HinhAnh = '/images/NhaGiaKim.jpg' WHERE MaSach = 1;

-- 2. Cập nhật ảnh cho cuốn 'Đắc Nhân Tâm' (MaSach = 2)
UPDATE Sach SET HinhAnh = '/images/DacNhanTam.jpg' WHERE MaSach = 2;

-- 3. Cập nhật ảnh cho cuốn 'Lược Sử Thời Gian' (MaSach = 3)
UPDATE Sach SET HinhAnh = '/images/LuocSuThoiGian.jpg' WHERE MaSach = 3;

-- 4. Cập nhật ảnh cho cuốn 'Bí Mật Tư Duy Triệu Phú' (MaSach = 4)
UPDATE Sach SET HinhAnh = '/images/BiMatTuDuyTrieuPhu.jpg' WHERE MaSach = 4;
SELECT MaSach, HinhAnh FROM Sach WHERE MaSach IN (1, 2, 3, 4);