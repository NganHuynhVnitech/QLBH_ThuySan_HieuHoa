USE HieuHoaDB;
GO

-- 1. Ensure columns exist for all master data tables
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('KhachHang') AND name = 'isDisabled')
BEGIN
    ALTER TABLE KhachHang ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('HangHoa') AND name = 'isDisabled')
BEGIN
    ALTER TABLE HangHoa ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NhaCungCap') AND name = 'isDisabled')
BEGIN
    ALTER TABLE NhaCungCap ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

-- 2. Update CRUD Stored Procedures to support soft delete and filter disabled records

-- =============================================
-- KHACH HANG
-- =============================================
CREATE OR ALTER PROCEDURE sp_KhachHang_Insert 
    @maDoiTuong VARCHAR(20), 
    @tenDoiTuong NVARCHAR(100), 
    @soDienThoai VARCHAR(20), 
    @diaChi NVARCHAR(200), 
    @aoNuoi NVARCHAR(100), 
    @isDisabled BIT = 0 
AS
BEGIN
    INSERT INTO KhachHang(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi, isDisabled) 
    VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @aoNuoi, @isDisabled);
END
GO

CREATE OR ALTER PROCEDURE sp_KhachHang_Update 
    @maDoiTuong VARCHAR(20), 
    @tenDoiTuong NVARCHAR(100), 
    @soDienThoai VARCHAR(20), 
    @diaChi NVARCHAR(200), 
    @aoNuoi NVARCHAR(100), 
    @isDisabled BIT = 0 
AS
BEGIN
    UPDATE KhachHang 
    SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, aoNuoi=@aoNuoi, isDisabled=@isDisabled 
    WHERE maDoiTuong=@maDoiTuong;
END
GO

CREATE OR ALTER PROCEDURE sp_KhachHang_SelectAll 
AS 
BEGIN
    SELECT * FROM KhachHang WHERE isDisabled = 0;
END
GO

-- Updated to Soft Delete
CREATE OR ALTER PROCEDURE sp_KhachHang_Delete 
    @maDoiTuong VARCHAR(20) 
AS 
BEGIN
    UPDATE KhachHang SET isDisabled = 1 WHERE maDoiTuong = @maDoiTuong;
END
GO

-- =============================================
-- HANG HOA
-- =============================================
CREATE OR ALTER PROCEDURE sp_HangHoa_Insert 
    @maHang VARCHAR(20), 
    @tenHang NVARCHAR(100), 
    @donViTinh NVARCHAR(20), 
    @quyCach NVARCHAR(50), 
    @isDisabled BIT = 0 
AS
BEGIN
    INSERT INTO HangHoa(maHang, tenHang, donViTinh, quyCach, isDisabled) 
    VALUES (@maHang, @tenHang, @donViTinh, @quyCach, @isDisabled);
END
GO

CREATE OR ALTER PROCEDURE sp_HangHoa_Update 
    @maHang VARCHAR(20), 
    @tenHang NVARCHAR(100), 
    @donViTinh NVARCHAR(20), 
    @quyCach NVARCHAR(50), 
    @isDisabled BIT = 0 
AS
BEGIN
    UPDATE HangHoa 
    SET tenHang=@tenHang, donViTinh=@donViTinh, quyCach=@quyCach, isDisabled=@isDisabled 
    WHERE maHang=@maHang;
END
GO

CREATE OR ALTER PROCEDURE sp_HangHoa_SelectAll 
AS 
BEGIN
    SELECT * FROM HangHoa WHERE isDisabled = 0;
END
GO

-- Updated to Soft Delete
CREATE OR ALTER PROCEDURE sp_HangHoa_Delete 
    @maHang VARCHAR(20) 
AS 
BEGIN
    UPDATE HangHoa SET isDisabled = 1 WHERE maHang = @maHang;
END
GO

-- =============================================
-- NHA CUNG CAP
-- =============================================
CREATE OR ALTER PROCEDURE sp_NhaCungCap_Insert 
    @maDoiTuong VARCHAR(20), 
    @tenDoiTuong NVARCHAR(100), 
    @soDienThoai VARCHAR(20), 
    @diaChi NVARCHAR(200), 
    @maSoThue VARCHAR(50), 
    @soNgayDuocNo INT, 
    @isDisabled BIT = 0 
AS
BEGIN
    INSERT INTO NhaCungCap(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo, isDisabled) 
    VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @maSoThue, @soNgayDuocNo, @isDisabled);
END
GO

CREATE OR ALTER PROCEDURE sp_NhaCungCap_Update 
    @maDoiTuong VARCHAR(20), 
    @tenDoiTuong NVARCHAR(100), 
    @soDienThoai VARCHAR(20), 
    @diaChi NVARCHAR(200), 
    @maSoThue VARCHAR(50), 
    @soNgayDuocNo INT, 
    @isDisabled BIT = 0 
AS
BEGIN
    UPDATE NhaCungCap 
    SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, maSoThue=@maSoThue, soNgayDuocNo=@soNgayDuocNo, isDisabled=@isDisabled 
    WHERE maDoiTuong=@maDoiTuong;
END
GO

CREATE OR ALTER PROCEDURE sp_NhaCungCap_SelectAll 
AS 
BEGIN
    SELECT * FROM NhaCungCap WHERE isDisabled = 0;
END
GO

-- Updated to Soft Delete
CREATE OR ALTER PROCEDURE sp_NhaCungCap_Delete 
    @maDoiTuong VARCHAR(20) 
AS 
BEGIN
    UPDATE NhaCungCap SET isDisabled = 1 WHERE maDoiTuong = @maDoiTuong;
END
GO
