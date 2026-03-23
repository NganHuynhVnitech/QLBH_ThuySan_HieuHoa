USE HieuHoaDB;
GO

-- Add isDisabled column to Remaining Master Data Tables
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DaiLy') AND name = 'isDisabled')
BEGIN
    ALTER TABLE DaiLy ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Kho') AND name = 'isDisabled')
BEGIN
    ALTER TABLE Kho ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DoiTuongChiPhi') AND name = 'isDisabled')
BEGIN
    ALTER TABLE DoiTuongChiPhi ADD isDisabled BIT NOT NULL DEFAULT 0;
END
GO
