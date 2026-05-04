
-- Patch Script: Add Payment Date Column to Import/Export Tables
-- Use this to update your EXISTING database without recreating tables.

USE HieuHoaDB;
GO

-- 1. Update PhieuNhap Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhieuNhap' AND COLUMN_NAME = 'ngayThanhToan')
BEGIN
    ALTER TABLE PhieuNhap ADD ngayThanhToan DATETIME NULL;
    PRINT 'Added column ngayThanhToan to PhieuNhap';
END
ELSE
BEGIN
    PRINT 'Column ngayThanhToan already exists in PhieuNhap';
END
GO

-- 2. Update PhieuXuat Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhieuXuat' AND COLUMN_NAME = 'ngayThanhToan')
BEGIN
    ALTER TABLE PhieuXuat ADD ngayThanhToan DATETIME NULL;
    PRINT 'Added column ngayThanhToan to PhieuXuat';
END
ELSE
BEGIN
    PRINT 'Column ngayThanhToan already exists in PhieuXuat';
END
GO
