
-- Patch Script: Add Table DoiTuongChiPhi (Cost Objects)
-- Use this to update your EXISTING database without recreating tables.

USE HieuHoaDB;
GO

-- 1. Create Table if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DoiTuongChiPhi')
BEGIN
    CREATE TABLE DoiTuongChiPhi (
        maDoiTuong VARCHAR(20) PRIMARY KEY,
        tenDoiTuong NVARCHAR(100) NOT NULL
    );
    PRINT 'Created table DoiTuongChiPhi';
END
ELSE
BEGIN
    PRINT 'Table DoiTuongChiPhi already exists';
END
GO

-- 2. Seed Data
IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP001')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP001', N'Tiền Điện');

IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP002')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP002', N'Tiền Nước');

IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP003')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP003', N'Tiền Lương Nhân Viên');

IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP004')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP004', N'Chi Phí Vận Chuyển');

PRINT 'Seeding completed for DoiTuongChiPhi';
GO
