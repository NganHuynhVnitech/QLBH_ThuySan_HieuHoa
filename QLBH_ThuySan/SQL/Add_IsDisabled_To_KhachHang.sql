-- Add isDisabled column to KhachHang table
-- Default value 0 (false)
ALTER TABLE KhachHang ADD isDisabled BIT NOT NULL DEFAULT 0;
GO
