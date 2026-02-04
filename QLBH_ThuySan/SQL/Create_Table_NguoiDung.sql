CREATE TABLE [dbo].[NguoiDung](
	[TenNguoiDung] [varchar](50) NOT NULL,
	[TenHienThi] [nvarchar](50) NULL,
	[MatKhau] [nvarchar](50) NULL,
	[QuyenNguoiDung] [int] NULL,
 CONSTRAINT [PK_NguoiDung] PRIMARY KEY CLUSTERED 
(
	[TenNguoiDung] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- Insert default admin user if not exists
IF NOT EXISTS (SELECT * FROM NguoiDung WHERE TenNguoiDung = 'admin')
BEGIN
    INSERT INTO NguoiDung (TenNguoiDung, TenHienThi, MatKhau, QuyenNguoiDung)
    VALUES ('admin', N'Quản trị viên', 'admin123', 1)
END
GO

-- Insert default manager user if not exists
IF NOT EXISTS (SELECT * FROM NguoiDung WHERE TenNguoiDung = 'manager')
BEGIN
    INSERT INTO NguoiDung (TenNguoiDung, TenHienThi, MatKhau, QuyenNguoiDung)
    VALUES ('manager', N'Quản lý', 'manager123', 2)
END
GO

-- Insert default saler user if not exists
IF NOT EXISTS (SELECT * FROM NguoiDung WHERE TenNguoiDung = 'saler')
BEGIN
    INSERT INTO NguoiDung (TenNguoiDung, TenHienThi, MatKhau, QuyenNguoiDung)
    VALUES ('saler', N'Nhân viên bán hàng', 'saler123', 3)
END
GO
