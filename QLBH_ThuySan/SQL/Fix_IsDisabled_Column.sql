-- Update all master data tables to support soft delete (isDisabled)
-- Run this script on HieuHoaDB to fix the 'Invalid column name isDisabled' error

USE HieuHoaDB;
GO

-- 1. Update KhachHang
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('KhachHang') AND name = 'isDisabled')
BEGIN
    ALTER TABLE KhachHang ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

-- 2. Update HangHoa
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('HangHoa') AND name = 'isDisabled')
BEGIN
    ALTER TABLE HangHoa ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

-- 3. Update NhaCungCap
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NhaCungCap') AND name = 'isDisabled')
BEGIN
    ALTER TABLE NhaCungCap ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

-- Update Stored Procedures (Redo from SQLData.sql)
-- sp_KhachHang_Insert
CREATE OR ALTER PROCEDURE sp_KhachHang_Insert @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @aoNuoi NVARCHAR(100), @isDisabled BIT = 0 AS
INSERT INTO KhachHang(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi, isDisabled) VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @aoNuoi, @isDisabled);
GO

-- sp_KhachHang_Update
CREATE OR ALTER PROCEDURE sp_KhachHang_Update @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @aoNuoi NVARCHAR(100), @isDisabled BIT = 0 AS
UPDATE KhachHang SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, aoNuoi=@aoNuoi, isDisabled=@isDisabled WHERE maDoiTuong=@maDoiTuong;
GO

-- sp_KhachHang_SelectAll
CREATE OR ALTER PROCEDURE sp_KhachHang_SelectAll AS SELECT * FROM KhachHang WHERE isDisabled = 0;
GO

-- sp_HangHoa_Insert
CREATE OR ALTER PROCEDURE sp_HangHoa_Insert @maHang VARCHAR(20), @tenHang NVARCHAR(100), @donViTinh NVARCHAR(20), @quyCach NVARCHAR(50), @isDisabled BIT = 0 AS
INSERT INTO HangHoa(maHang, tenHang, donViTinh, quyCach, isDisabled) VALUES (@maHang, @tenHang, @donViTinh, @quyCach, @isDisabled);
GO

-- sp_HangHoa_Update
CREATE OR ALTER PROCEDURE sp_HangHoa_Update @maHang VARCHAR(20), @tenHang NVARCHAR(100), @donViTinh NVARCHAR(20), @quyCach NVARCHAR(50), @isDisabled BIT = 0 AS
UPDATE HangHoa SET tenHang=@tenHang, donViTinh=@donViTinh, quyCach=@quyCach, isDisabled=@isDisabled WHERE maHang=@maHang;
GO

-- sp_HangHoa_SelectAll
CREATE OR ALTER PROCEDURE sp_HangHoa_SelectAll AS SELECT * FROM HangHoa WHERE isDisabled = 0;
GO

-- sp_NhaCungCap_Insert
CREATE OR ALTER PROCEDURE sp_NhaCungCap_Insert @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @maSoThue VARCHAR(50), @soNgayDuocNo INT, @isDisabled BIT = 0 AS
INSERT INTO NhaCungCap(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo, isDisabled) VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @maSoThue, @soNgayDuocNo, @isDisabled);
GO

-- sp_NhaCungCap_Update
CREATE OR ALTER PROCEDURE sp_NhaCungCap_Update @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @maSoThue VARCHAR(50), @soNgayDuocNo INT, @isDisabled BIT = 0 AS
UPDATE NhaCungCap SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, maSoThue=@maSoThue, soNgayDuocNo=@soNgayDuocNo, isDisabled=@isDisabled WHERE maDoiTuong=@maDoiTuong;
GO

-- sp_NhaCungCap_SelectAll
CREATE OR ALTER PROCEDURE sp_NhaCungCap_SelectAll AS SELECT * FROM NhaCungCap WHERE isDisabled = 0;
GO
