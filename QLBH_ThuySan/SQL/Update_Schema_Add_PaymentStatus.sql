
-- Patch Script: Add trangthaithanhtoan and Update Stored Procedures
-- Apply this to HieuHoaDB

USE HieuHoaDB;
GO

-- 1. Add trangthaithanhtoan column to PhieuNhap
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhieuNhap' AND COLUMN_NAME = 'trangthaithanhtoan')
BEGIN
    ALTER TABLE PhieuNhap ADD trangthaithanhtoan NVARCHAR(50) DEFAULT N'Chưa Thanh Toán';
    PRINT 'Added column trangthaithanhtoan to PhieuNhap';
END
GO

-- 2. Add trangthaithanhtoan column to PhieuXuat
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhieuXuat' AND COLUMN_NAME = 'trangthaithanhtoan')
BEGIN
    ALTER TABLE PhieuXuat ADD trangthaithanhtoan NVARCHAR(50) DEFAULT N'Chưa Thanh Toán';
    PRINT 'Added column trangthaithanhtoan to PhieuXuat';
END
GO

-- 3. Update sp_PhieuNhap_Insert
IF OBJECT_ID('sp_PhieuNhap_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuNhap_Insert;
GO
CREATE PROCEDURE sp_PhieuNhap_Insert 
    @maPhieu VARCHAR(20), 
    @ngayNhap DATETIME, 
    @idDaiLyNhap VARCHAR(20), 
    @idNhaCungCap VARCHAR(20),
    @trangthaithanhtoan NVARCHAR(50) = N'Chưa Thanh Toán'
AS
BEGIN
    INSERT INTO PhieuNhap(maPhieu, ngayNhap, idDaiLyNhap, idNhaCungCap, tongTien, trangthaithanhtoan) 
    VALUES (@maPhieu, @ngayNhap, @idDaiLyNhap, @idNhaCungCap, 0, @trangthaithanhtoan);
END
GO
PRINT 'Updated sp_PhieuNhap_Insert';
GO

-- 4. Update sp_PhieuXuat_Insert
IF OBJECT_ID('sp_PhieuXuat_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuXuat_Insert;
GO
CREATE PROCEDURE sp_PhieuXuat_Insert 
    @maPhieu VARCHAR(20), 
    @ngayXuat DATETIME, 
    @idDaiLyBan VARCHAR(20), 
    @idKhachHang VARCHAR(20),
    @trangthaithanhtoan NVARCHAR(50) = N'Chưa Thanh Toán'
AS
BEGIN
    INSERT INTO PhieuXuat(maPhieu, ngayXuat, idDaiLyBan, idKhachHang, tongTien, trangthaithanhtoan) 
    VALUES (@maPhieu, @ngayXuat, @idDaiLyBan, @idKhachHang, 0, @trangthaithanhtoan);
END
GO
PRINT 'Updated sp_PhieuXuat_Insert';
GO
