
-- Patch Script: Add maDoiTuong and Update Check Constraint for PhieuThuChi
-- Apply this to HieuHoaDB

USE HieuHoaDB;
GO

-- 1. Add maDoiTuong column if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PhieuThuChi' AND COLUMN_NAME = 'maDoiTuong')
BEGIN
    ALTER TABLE PhieuThuChi ADD maDoiTuong VARCHAR(20) NULL;
    PRINT 'Added column maDoiTuong to PhieuThuChi';
END
ELSE
BEGIN
    PRINT 'Column maDoiTuong already exists';
END
GO

-- 2. Update Check Constraint for loaiPhieu
-- First, drop existing constraint if possible. Finding constraint name is tricky if default.
-- Assuming standard manual constraint or recreate table if simpler, but here we use ALTER.

-- Ideally, we'd look up the constraint name.
DECLARE @constraintName NVARCHAR(200);
SELECT @constraintName = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('PhieuThuChi') 
AND definition LIKE '%loaiPhieu%';

IF @constraintName IS NOT NULL
BEGIN
    PRINT 'Dropping constraint: ' + @constraintName;
    EXEC('ALTER TABLE PhieuThuChi DROP CONSTRAINT ' + @constraintName);
END
GO

-- Add new constraint
ALTER TABLE PhieuThuChi ADD CONSTRAINT CK_PhieuThuChi_LoaiPhieu CHECK (loaiPhieu IN ('THU', 'CHI', 'NHAP'));
PRINT 'Updated check constraint for loaiPhieu (THU, CHI, NHAP)';
GO
