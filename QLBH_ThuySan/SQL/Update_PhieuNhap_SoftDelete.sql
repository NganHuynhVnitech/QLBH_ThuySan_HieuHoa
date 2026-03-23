USE HieuHoaDB;
GO

-- 1. Add isDisabled column to PhieuNhap
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PhieuNhap') AND name = 'isDisabled')
BEGIN
    ALTER TABLE PhieuNhap ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

-- 2. Update sp_PhieuNhap_Delete to perform soft delete
CREATE OR ALTER PROCEDURE sp_PhieuNhap_Delete 
    @maPhieu VARCHAR(20) 
AS
BEGIN
    -- We don't delete from ChiTietPhieuNhap to preserve history
    -- Just mark the main PhieuNhap as disabled
    UPDATE PhieuNhap SET isDisabled = 1 WHERE maPhieu = @maPhieu;
END
GO

-- 3. Update sp_PhieuNhap_SelectAll to filter out disabled records
CREATE OR ALTER PROCEDURE sp_PhieuNhap_SelectAll
AS
BEGIN
    SELECT * FROM PhieuNhap WHERE isDisabled = 0;
END
GO
