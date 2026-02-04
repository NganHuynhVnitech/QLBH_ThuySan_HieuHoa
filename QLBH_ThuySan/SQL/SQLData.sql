-- =============================================
-- DATABASE SETUP: CỬA HÀNG THỦY SẢN HIỆU HOA
-- Dựa trên Class Diagram Docx [1-10]
-- =============================================

USE master;
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'HieuHoaDB')
    DROP DATABASE HieuHoaDB;
GO
CREATE DATABASE HieuHoaDB;
GO
USE HieuHoaDB;
GO

-- =============================================
-- 1. DROP TABLES IF EXIST (Clean up)
-- =============================================
IF OBJECT_ID('ChiTietPhieuXuat', 'U') IS NOT NULL DROP TABLE ChiTietPhieuXuat;
IF OBJECT_ID('PhieuXuat', 'U') IS NOT NULL DROP TABLE PhieuXuat;
IF OBJECT_ID('ChiTietPhieuNhap', 'U') IS NOT NULL DROP TABLE ChiTietPhieuNhap;
IF OBJECT_ID('PhieuNhap', 'U') IS NOT NULL DROP TABLE PhieuNhap;
IF OBJECT_ID('ChiTietTon', 'U') IS NOT NULL DROP TABLE ChiTietTon;
IF OBJECT_ID('Kho', 'U') IS NOT NULL DROP TABLE Kho;
IF OBJECT_ID('BangKeChietKhau', 'U') IS NOT NULL DROP TABLE BangKeChietKhau;
IF OBJECT_ID('PhieuTinhChietKhau', 'U') IS NOT NULL DROP TABLE PhieuTinhChietKhau;
IF OBJECT_ID('CauHinhChietKhau', 'U') IS NOT NULL DROP TABLE CauHinhChietKhau;
IF OBJECT_ID('SoRiengKhachHang', 'U') IS NOT NULL DROP TABLE SoRiengKhachHang;
IF OBJECT_ID('PhieuThuChi', 'U') IS NOT NULL DROP TABLE PhieuThuChi;
IF OBJECT_ID('HangHoa', 'U') IS NOT NULL DROP TABLE HangHoa;
IF OBJECT_ID('DaiLy', 'U') IS NOT NULL DROP TABLE DaiLy;
IF OBJECT_ID('KhachHang', 'U') IS NOT NULL DROP TABLE KhachHang;
IF OBJECT_ID('NhaCungCap', 'U') IS NOT NULL DROP TABLE NhaCungCap;

-- =============================================
-- 2. CREATE TABLES (MASTER DATA) [1, 3]
-- =============================================

-- Class: HangHoa [1]
CREATE TABLE HangHoa (
    maHang VARCHAR(20) PRIMARY KEY,
    tenHang NVARCHAR(100) NOT NULL,
    donViTinh NVARCHAR(20),
    quyCach NVARCHAR(50),
    giaVonHienTai DECIMAL(18, 2) DEFAULT 0, -- Cập nhật bởi COGS Engine
    giaBanHienTai DECIMAL(18, 2) DEFAULT 0
);

-- Class: NhaCungCap (Inherits DoiTuong) [3]
CREATE TABLE NhaCungCap (
    maDoiTuong VARCHAR(20) PRIMARY KEY,
    tenDoiTuong NVARCHAR(100),
    soDienThoai VARCHAR(20),
    diaChi NVARCHAR(200),
    maSoThue VARCHAR(50),
    soNgayDuocNo INT DEFAULT 0 -- Quan trọng để tính hạn thanh toán
);

-- Class: KhachHang (Inherits DoiTuong) [3]
CREATE TABLE KhachHang (
    maDoiTuong VARCHAR(20) PRIMARY KEY,
    tenDoiTuong NVARCHAR(100),
    soDienThoai VARCHAR(20),
    diaChi NVARCHAR(200),
    aoNuoi NVARCHAR(100),
    duNoLuyKe DECIMAL(18, 2) DEFAULT 0 -- Liên kết SoRieng
);

-- Class: DaiLy (Inherits DoiTuong) [3]
CREATE TABLE DaiLy (
    maDaiLy VARCHAR(20) PRIMARY KEY, -- Mapping maDoiTuong
    tenDaiLy NVARCHAR(100),
    loaiDaiLy VARCHAR(20) CHECK (loaiDaiLy IN ('NHAP_A', 'NHAP_B', 'BAN_C'))
);

-- =============================================
-- 3. WAREHOUSE STRUCTURE [4, 5]
-- =============================================

-- Class: Kho (Abstract -> KhoVatLy + KhoTongAo) [4]
CREATE TABLE Kho (
    maKho VARCHAR(20) PRIMARY KEY,
    tenKho NVARCHAR(100),
    loaiKho VARCHAR(20) CHECK (loaiKho IN ('VAT_LY', 'TONG_AO')),
    maDaiLyPhuTrach VARCHAR(20) NULL, -- Link to DaiLy if VAT_LY
    FOREIGN KEY (maDaiLyPhuTrach) REFERENCES DaiLy(maDaiLy)
);

-- Class: ChiTietTon [5]
CREATE TABLE ChiTietTon (
    maKho VARCHAR(20),
    maHang VARCHAR(20),
    soLuongTon FLOAT DEFAULT 0,
    giaTriTon DECIMAL(18, 2) DEFAULT 0, -- Chỉ dùng cho Kho Tổng Ảo
    PRIMARY KEY (maKho, maHang),
    FOREIGN KEY (maKho) REFERENCES Kho(maKho),
    FOREIGN KEY (maHang) REFERENCES HangHoa(maHang)
);

-- =============================================
-- 4. OPERATIONS (NHẬP - XUẤT) [6, 7]
-- =============================================

-- Class: PhieuNhap [6]
CREATE TABLE PhieuNhap (
    maPhieu VARCHAR(20) PRIMARY KEY,
    ngayNhap DATETIME DEFAULT GETDATE(),
    idDaiLyNhap VARCHAR(20),
    idNhaCungCap VARCHAR(20),
    hanThanhToan DATETIME, -- Tính toán: ngayNhap + soNgayDuocNo
    tongTien DECIMAL(18, 2) DEFAULT 0,
    FOREIGN KEY (idDaiLyNhap) REFERENCES DaiLy(maDaiLy),
    FOREIGN KEY (idNhaCungCap) REFERENCES NhaCungCap(maDoiTuong)
);

-- Class: ChiTietPhieuNhap [6]
CREATE TABLE ChiTietPhieuNhap (
    maPhieu VARCHAR(20),
    maHang VARCHAR(20),
    soLuong FLOAT,
    donGiaNhap DECIMAL(18, 2),
    thanhTien AS (soLuong * donGiaNhap),
    PRIMARY KEY (maPhieu, maHang),
    FOREIGN KEY (maPhieu) REFERENCES PhieuNhap(maPhieu),
    FOREIGN KEY (maHang) REFERENCES HangHoa(maHang)
);

-- Class: PhieuXuat [7]
CREATE TABLE PhieuXuat (
    maPhieu VARCHAR(20) PRIMARY KEY,
    ngayXuat DATETIME DEFAULT GETDATE(),
    idDaiLyBan VARCHAR(20),
    idKhachHang VARCHAR(20),
    tongTien DECIMAL(18, 2) DEFAULT 0,
    FOREIGN KEY (idDaiLyBan) REFERENCES DaiLy(maDaiLy),
    FOREIGN KEY (idKhachHang) REFERENCES KhachHang(maDoiTuong)
);

-- Class: ChiTietPhieuXuat [7]
CREATE TABLE ChiTietPhieuXuat (
    maPhieu VARCHAR(20),
    maHang VARCHAR(20),
    soLuong FLOAT,
    giaBan DECIMAL(18, 2),
    giaVonTaiThoiDiem DECIMAL(18, 2), -- Snapshot giá vốn lúc xuất
    thanhTien AS (soLuong * giaBan),
    PRIMARY KEY (maPhieu, maHang),
    FOREIGN KEY (maPhieu) REFERENCES PhieuXuat(maPhieu),
    FOREIGN KEY (maHang) REFERENCES HangHoa(maHang)
);

-- =============================================
-- 5. DISCOUNT & FINANCE [2, 8-10]
-- =============================================

-- Class: CauHinhChietKhau (ChienLuocChietKhau implement) [8, 9]
CREATE TABLE CauHinhChietKhau (
    id INT IDENTITY(1,1) PRIMARY KEY,
    tenCauHinh NVARCHAR(100),
    loaiQuyLuat VARCHAR(20) CHECK (loaiQuyLuat IN ('THEO_BAC', 'CO_DINH')),
    doiTuongApDung VARCHAR(20) CHECK (doiTuongApDung IN ('NCC', 'KHACH')),
    -- JSON hoặc XML lưu chi tiết các mốc (List<MocSanLuong> trong diagram)
    chiTietLuat NVARCHAR(MAX) 
);

-- Class: PhieuTinhChietKhau [8]
CREATE TABLE PhieuTinhChietKhau (
    maPhieuTinh VARCHAR(20) PRIMARY KEY,
    loaiDoiTuong VARCHAR(10) CHECK (loaiDoiTuong IN ('NCC', 'KHACH')),
    tuNgay DATE,
    denNgay DATE,
    ngayTao DATETIME DEFAULT GETDATE()
);

-- Class: BangKeChietKhau [9]
CREATE TABLE BangKeChietKhau (
    maBangKe VARCHAR(20) PRIMARY KEY,
    maPhieuTinh VARCHAR(20),
    maDoiTuong VARCHAR(20), -- NCC hoặc Khách
    tongTien DECIMAL(18, 2),
    trangThai VARCHAR(20) DEFAULT 'Pending', -- Pending/Approved
    FOREIGN KEY (maPhieuTinh) REFERENCES PhieuTinhChietKhau(maPhieuTinh)
);

-- Class: SoRiengKhachHang (Lưu lịch sử giao dịch) [10]
CREATE TABLE SoRiengKhachHang (
    id INT IDENTITY(1,1) PRIMARY KEY,
    maKhachHang VARCHAR(20),
    ngayGiaoDich DATETIME,
    loaiGiaoDich VARCHAR(20) CHECK (loaiGiaoDich IN ('MUA_HANG', 'THANH_TOAN', 'CAN_TRU')),
    soTienPhatSinh DECIMAL(18, 2), -- Dương: Tăng nợ, Âm: Giảm nợ
    dienGiai NVARCHAR(200),
    FOREIGN KEY (maKhachHang) REFERENCES KhachHang(maDoiTuong)
);

-- Class: PhieuThuChi [2]
CREATE TABLE PhieuThuChi (
    maPhieu VARCHAR(20) PRIMARY KEY,
    loaiPhieu VARCHAR(10) CHECK (loaiPhieu IN ('THU', 'CHI')),
    ngayLap DATETIME DEFAULT GETDATE(),
    soTien DECIMAL(18, 2),
    lyDo NVARCHAR(200) -- Thu bán hàng, Chi trả NCC...
);

GO

-- =============================================
-- 6. STORED PROCEDURES (MAPPING CLASS METHODS)
-- =============================================

-- Method: KhoTongAo.tinhGiaVonBinhQuanLienHoan() [4]
-- Method: PhieuNhap.triggerDongBoKho() [6]
-- Logic: Cập nhật tồn kho vật lý + kho tổng + tính lại giá vốn
CREATE PROCEDURE sp_PhieuNhap_DongBoVaTinhGia
    @maPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @maKhoVatLy VARCHAR(20);
    DECLARE @maKhoTong VARCHAR(20) = 'KHO_TONG_AO'; 
    DECLARE @idDaiLy VARCHAR(20);

    SELECT @idDaiLy = idDaiLyNhap FROM PhieuNhap WHERE maPhieu = @maPhieu;
    -- Giả định mapping: ID Đại lý = ID Kho Vật lý
    SET @maKhoVatLy = @idDaiLy; 

    -- 1. Cập nhật Kho Vật Lý (Chỉ tăng số lượng)
    MERGE INTO ChiTietTon AS Target
    USING (SELECT maHang, soLuong FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu) AS Source
    ON Target.maKho = @maKhoVatLy AND Target.maHang = Source.maHang
    WHEN MATCHED THEN
        UPDATE SET soLuongTon = soLuongTon + Source.soLuong
    WHEN NOT MATCHED THEN
        INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoVatLy, Source.maHang, Source.soLuong, 0);

    -- 2. Cập nhật Kho Tổng & Tính Giá Vốn (Bình quân gia quyền liên hoàn)
    -- Công thức: Giá Mới = (Giá Trị Cũ + Giá Trị Nhập Mới) / (SL Cũ + SL Nhập Mới)
    
    DECLARE cur CURSOR FOR SELECT maHang, soLuong, donGiaNhap FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu;
    DECLARE @maHang VARCHAR(20), @slNhap FLOAT, @giaNhap DECIMAL(18,2);
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Lấy dữ liệu cũ từ Kho Tổng
        DECLARE @slCu FLOAT = 0;
        DECLARE @giaVonCu DECIMAL(18,2) = 0;
        
        SELECT @slCu = soLuongTon, @giaVonCu = giaTriTon 
        FROM ChiTietTon WHERE maKho = @maKhoTong AND maHang = @maHang;

        IF @slCu IS NULL SET @slCu = 0;
        IF @giaVonCu IS NULL SET @giaVonCu = 0;

        -- Tính toán
        DECLARE @tongGiaTriMoi DECIMAL(18,2) = (@slCu * @giaVonCu) + (@slNhap * @giaNhap);
        DECLARE @tongSlMoi FLOAT = @slCu + @slNhap;
        DECLARE @giaVonMoi DECIMAL(18,2) = 0;

        IF @tongSlMoi > 0 
            SET @giaVonMoi = @tongGiaTriMoi / @tongSlMoi;
        
        -- Cập nhật Kho Tổng
        MERGE INTO ChiTietTon AS Target
        USING (SELECT @maHang AS maHang) AS Source
        ON Target.maKho = @maKhoTong AND Target.maHang = Source.maHang
        WHEN MATCHED THEN
            UPDATE SET soLuongTon = @tongSlMoi, giaTriTon = @giaVonMoi
        WHEN NOT MATCHED THEN
            INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoTong, @maHang, @tongSlMoi, @giaVonMoi);
            
        -- Cập nhật lại giá vốn vào Master Hàng Hóa để query nhanh
        UPDATE HangHoa SET giaVonHienTai = @giaVonMoi WHERE maHang = @maHang;

        FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END;
GO

-- Method: PhieuNhap.tinhHanThanhToan() [6]
-- Trigger tự động tính hạn thanh toán khi Insert PhieuNhap
CREATE TRIGGER trg_PhieuNhap_TinhHanThanhToan
ON PhieuNhap
AFTER INSERT
AS
BEGIN
    UPDATE pn
    SET hanThanhToan = DATEADD(DAY, ncc.soNgayDuocNo, pn.ngayNhap)
    FROM PhieuNhap pn
    JOIN Inserted i ON pn.maPhieu = i.maPhieu
    JOIN NhaCungCap ncc ON pn.idNhaCungCap = ncc.maDoiTuong;
END;
GO

-- Method: SoRiengKhachHang.capNhatPhatSinhNo() [10]
-- Method: KhoTongAo.layGiaVonHienTai (cho PhieuXuat) [7]
CREATE PROCEDURE sp_PhieuXuat_XuLy
    @maPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Snapshot Giá Vốn & Update Chi Tiết Phiếu Xuất
    UPDATE ctp
    SET giaVonTaiThoiDiem = hh.giaVonHienTai
    FROM ChiTietPhieuXuat ctp
    JOIN HangHoa hh ON ctp.maHang = hh.maHang
    WHERE ctp.maPhieu = @maPhieu;

    -- 2. Giảm Tồn Kho Vật Lý & Kho Tổng
    DECLARE @idDaiLy VARCHAR(20), @idKhachHang VARCHAR(20), @ngayXuat DATETIME, @tongTien DECIMAL(18,2);
    SELECT @idDaiLy = idDaiLyBan, @idKhachHang = idKhachHang, @ngayXuat = ngayXuat FROM PhieuXuat WHERE maPhieu = @maPhieu;

    -- Update Kho (Logic giản lược: Trừ thẳng)
    UPDATE t
    SET t.soLuongTon = t.soLuongTon - ctp.soLuong
    FROM ChiTietTon t
    JOIN ChiTietPhieuXuat ctp ON t.maHang = ctp.maHang
    WHERE ctp.maPhieu = @maPhieu AND t.maKho IN (@idDaiLy, 'KHO_TONG_AO');

    -- 3. Ghi Nợ Sổ Riêng
    SELECT @tongTien = SUM(thanhTien) FROM ChiTietPhieuXuat WHERE maPhieu = @maPhieu;
    
    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai)
    VALUES (@idKhachHang, @ngayXuat, 'MUA_HANG', @tongTien, N'Phát sinh từ phiếu xuất ' + @maPhieu);

    -- Update Dư nợ lũy kế master
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe + @tongTien WHERE maDoiTuong = @idKhachHang;
    
    -- Update Tong tien Header
    UPDATE PhieuXuat SET tongTien = @tongTien WHERE maPhieu = @maPhieu;
END;
GO

-- Method: PhieuTinhChietKhau.tinhToan() [8]
-- Engine Tính toán chiết khấu (Rất phức tạp, đây là khung sườn Logic)
CREATE PROCEDURE sp_Engine_TinhChietKhau
    @maPhieuTinh VARCHAR(20)
AS
BEGIN
    DECLARE @loaiDoiTuong VARCHAR(10), @tuNgay DATE, @denNgay DATE;
    SELECT @loaiDoiTuong = loaiDoiTuong, @tuNgay = tuNgay, @denNgay = denNgay 
    FROM PhieuTinhChietKhau WHERE maPhieuTinh = @maPhieuTinh;

    IF @loaiDoiTuong = 'NCC'
    BEGIN
        -- Logic: Quét PhieuNhap -> Group by NCC, HangHoa -> Sum(SoLuong)
        -- Áp dụng CauHinhChietKhau (Giả sử áp dụng Fixed Rule cho demo)
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + pn.idNhaCungCap,
            @maPhieuTinh,
            pn.idNhaCungCap,
            SUM(ct.soLuong * 1000), -- Ví dụ: 1000đ/kg (Cần join với CauHinhChietKhau để lấy số thực)
            'Pending'
        FROM PhieuNhap pn
        JOIN ChiTietPhieuNhap ct ON pn.maPhieu = ct.maPhieu
        WHERE pn.ngayNhap BETWEEN @tuNgay AND @denNgay
        GROUP BY pn.idNhaCungCap;
    END
    ELSE IF @loaiDoiTuong = 'KHACH'
    BEGIN
        -- Logic tương tự cho Khách hàng
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + px.idKhachHang,
            @maPhieuTinh,
            px.idKhachHang,
            SUM(ct.soLuong * 500), -- Ví dụ: 500đ/kg
            'Pending'
        FROM PhieuXuat px
        JOIN ChiTietPhieuXuat ct ON px.maPhieu = ct.maPhieu
        WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay
        GROUP BY px.idKhachHang;
    END
END;
GO

-- Method: QuanLyTaiChinh.canhBaoNhacNo() [2]
CREATE PROCEDURE sp_TaiChinh_CanhBaoNhacNo
AS
BEGIN
    SELECT 
        pn.maPhieu,
        ncc.tenDoiTuong AS NhaCungCap,
        pn.ngayNhap,
        pn.hanThanhToan,
        pn.tongTien,
        DATEDIFF(day, GETDATE(), pn.hanThanhToan) AS SoNgayConLai
    FROM PhieuNhap pn
    JOIN NhaCungCap ncc ON pn.idNhaCungCap = ncc.maDoiTuong
    WHERE pn.hanThanhToan <= DATEADD(day, 3, GETDATE()) -- Cảnh báo trước 3 ngày hoặc đã quá hạn
    ORDER BY pn.hanThanhToan ASC;
END;
GO

-- Method: QuanLyTaiChinh.tinhLoiNhuanThuc() [2]
-- Output: P&L Report
CREATE PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan
    @tuNgay DATE,
    @denNgay DATE
AS
BEGIN
    -- 1. Doanh Thu
    DECLARE @DoanhThu DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM PhieuXuat WHERE ngayXuat BETWEEN @tuNgay AND @denNgay);
    
    -- 2. Giá Vốn (Lấy từ snapshot lúc xuất)
    DECLARE @GiaVon DECIMAL(18,2) = (SELECT ISNULL(SUM(soLuong * giaVonTaiThoiDiem),0) 
                                     FROM ChiTietPhieuXuat ctp 
                                     JOIN PhieuXuat px ON ctp.maPhieu = px.maPhieu 
                                     WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay);
    
    -- 3. Chiết khấu NCC (Thu nhập khác)
    DECLARE @CK_NCC DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) 
                                     FROM BangKeChietKhau bk 
                                     JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                     WHERE pt.loaiDoiTuong = 'NCC' AND bk.trangThai = 'Approved');

    -- 4. Chiết khấu Khách (Chi phí bán hàng)
    DECLARE @CK_Khach DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) 
                                       FROM BangKeChietKhau bk 
                                       JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                       WHERE pt.loaiDoiTuong = 'KHACH' AND bk.trangThai = 'Approved');

    -- 5. Chi phí vận hành
    DECLARE @ChiPhiVH DECIMAL(18,2) = (SELECT ISNULL(SUM(soTien),0) FROM PhieuThuChi WHERE loaiPhieu = 'CHI' AND lyDo LIKE N'%Vận hành%');

    SELECT 
        @DoanhThu AS DoanhThu,
        @GiaVon AS GiaVonHangBan,
        @CK_NCC AS ThuNhapChietKhau,
        @CK_Khach AS ChiPhiChietKhau,
        @ChiPhiVH AS ChiPhiVanHanh,
        (@DoanhThu - @GiaVon + @CK_NCC - @CK_Khach - @ChiPhiVH) AS LoiNhuanRong;
END;
GO
USE HieuHoaDB;
GO

-- ====================================================================================
-- PHẦN A: CÁC STORED PROCEDURE XỬ LÝ NGHIỆP VỤ (BUSINESS LOGIC)
-- (Tương ứng với các Method trong Class Diagram: tính giá vốn, chiết khấu, sổ riêng...)
-- ====================================================================================

-- 1. Method: KhoTongAo.tinhGiaVonBinhQuanLienHoan() & triggerDongBoKho()
-- Mô tả: Cập nhật tồn kho vật lý, đồng bộ sang kho tổng và tính lại giá vốn ngay lập tức [Source 2, 6, 15, 23, 24]
IF OBJECT_ID('sp_PhieuNhap_DongBoVaTinhGia', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuNhap_DongBoVaTinhGia;
GO
CREATE PROCEDURE sp_PhieuNhap_DongBoVaTinhGia
    @maPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @maKhoVatLy VARCHAR(20);
    DECLARE @maKhoTong VARCHAR(20) = 'KHO_TONG_AO'; 
    DECLARE @idDaiLy VARCHAR(20);

    -- Lấy ID Đại lý nhập để xác định kho vật lý
    SELECT @idDaiLy = idDaiLyNhap FROM PhieuNhap WHERE maPhieu = @maPhieu;
    SET @maKhoVatLy = @idDaiLy; -- Giả định ID Đại lý map với ID Kho

    -- BƯỚC 1: Cập nhật Kho Vật Lý (Chỉ tăng số lượng thực tế)
    MERGE INTO ChiTietTon AS Target
    USING (SELECT maHang, soLuong FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu) AS Source
    ON Target.maKho = @maKhoVatLy AND Target.maHang = Source.maHang
    WHEN MATCHED THEN
        UPDATE SET soLuongTon = soLuongTon + Source.soLuong
    WHEN NOT MATCHED THEN
        INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoVatLy, Source.maHang, Source.soLuong, 0);

    -- BƯỚC 2: Tính toán COGS (Bình quân gia quyền liên hoàn) tại Kho Tổng
    DECLARE cur CURSOR FOR SELECT maHang, soLuong, donGiaNhap FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu;
    DECLARE @maHang VARCHAR(20), @slNhap FLOAT, @giaNhap DECIMAL(18,2);
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @slCu FLOAT = 0;
        DECLARE @giaVonCu DECIMAL(18,2) = 0;
        
        -- Lấy dữ liệu cũ từ Kho Tổng
        SELECT @slCu = soLuongTon, @giaVonCu = giaTriTon 
        FROM ChiTietTon WHERE maKho = @maKhoTong AND maHang = @maHang;

        SET @slCu = ISNULL(@slCu, 0);
        SET @giaVonCu = ISNULL(@giaVonCu, 0);

        -- Công thức Moving Weighted Average [Source 5]
        DECLARE @tongGiaTriMoi DECIMAL(18,2) = (@slCu * @giaVonCu) + (@slNhap * @giaNhap);
        DECLARE @tongSlMoi FLOAT = @slCu + @slNhap;
        DECLARE @giaVonMoi DECIMAL(18,2) = 0;

        IF @tongSlMoi > 0 SET @giaVonMoi = @tongGiaTriMoi / @tongSlMoi;
        
        -- Cập nhật lại Kho Tổng
        MERGE INTO ChiTietTon AS Target
        USING (SELECT @maHang AS maHang) AS Source
        ON Target.maKho = @maKhoTong AND Target.maHang = Source.maHang
        WHEN MATCHED THEN
            UPDATE SET soLuongTon = @tongSlMoi, giaTriTon = @giaVonMoi
        WHEN NOT MATCHED THEN
            INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoTong, @maHang, @tongSlMoi, @giaVonMoi);
            
        -- Cập nhật giá vốn master
        UPDATE HangHoa SET giaVonHienTai = @giaVonMoi WHERE maHang = @maHang;

        FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    END
    CLOSE cur; DEALLOCATE cur;
END;
GO

-- 2. Method: PhieuXuat.ghiNoSoRieng() & Kho.giamTon()
-- Mô tả: Xử lý phiếu xuất, snapshot giá vốn, trừ kho và ghi nợ sổ riêng [Source 6, 16, 26, 29]
IF OBJECT_ID('sp_PhieuXuat_XuLy', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuXuat_XuLy;
GO
CREATE PROCEDURE sp_PhieuXuat_XuLy
    @maPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Snapshot Giá Vốn (Lưu giá vốn tại thời điểm xuất để tính lãi lỗ chính xác)
    UPDATE ctp
    SET giaVonTaiThoiDiem = hh.giaVonHienTai
    FROM ChiTietPhieuXuat ctp
    JOIN HangHoa hh ON ctp.maHang = hh.maHang
    WHERE ctp.maPhieu = @maPhieu;

    -- 2. Giảm Tồn Kho (Vật lý & Tổng)
    DECLARE @idDaiLy VARCHAR(20), @idKhachHang VARCHAR(20), @ngayXuat DATETIME, @tongTien DECIMAL(18,2);
    SELECT @idDaiLy = idDaiLyBan, @idKhachHang = idKhachHang, @ngayXuat = ngayXuat FROM PhieuXuat WHERE maPhieu = @maPhieu;

    UPDATE t
    SET t.soLuongTon = t.soLuongTon - ctp.soLuong
    FROM ChiTietTon t
    JOIN ChiTietPhieuXuat ctp ON t.maHang = ctp.maHang
    WHERE ctp.maPhieu = @maPhieu AND t.maKho IN (@idDaiLy, 'KHO_TONG_AO');

    -- 3. Ghi Nợ Sổ Riêng
    SELECT @tongTien = SUM(soLuong * giaBan) FROM ChiTietPhieuXuat WHERE maPhieu = @maPhieu;
    UPDATE PhieuXuat SET tongTien = @tongTien WHERE maPhieu = @maPhieu;

    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai)
    VALUES (@idKhachHang, @ngayXuat, 'MUA_HANG', @tongTien, N'Phát sinh nợ từ phiếu ' + @maPhieu);

    -- Cập nhật dư nợ lũy kế Khách hàng
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe + @tongTien WHERE maDoiTuong = @idKhachHang;
END;
GO

-- 3. Method: PhieuTinhChietKhau.tinhToan() (Discount Engine)
-- Mô tả: Tính toán chiết khấu theo bậc hoặc cố định, tạo bảng kê Pending [Source 7-10, 17-18, 27-28]
IF OBJECT_ID('sp_Engine_TinhChietKhau', 'P') IS NOT NULL DROP PROCEDURE sp_Engine_TinhChietKhau;
GO
CREATE PROCEDURE sp_Engine_TinhChietKhau
    @maPhieuTinh VARCHAR(20)
AS
BEGIN
    DECLARE @loaiDoiTuong VARCHAR(10), @tuNgay DATE, @denNgay DATE;
    SELECT @loaiDoiTuong = loaiDoiTuong, @tuNgay = tuNgay, @denNgay = denNgay 
    FROM PhieuTinhChietKhau WHERE maPhieuTinh = @maPhieuTinh;

    -- Logic demo cho trường hợp Quy luật Cố định (Fixed Rate)
    IF @loaiDoiTuong = 'NCC'
    BEGIN
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + pn.idNhaCungCap,
            @maPhieuTinh,
            pn.idNhaCungCap,
            -- Giả định logic: Tổng SL * 1000đ (Cần join bảng CauHinhChietKhau để lấy tham số thực)
            SUM(ct.soLuong * 1000), 
            'Pending'
        FROM PhieuNhap pn
        JOIN ChiTietPhieuNhap ct ON pn.maPhieu = ct.maPhieu
        WHERE pn.ngayNhap BETWEEN @tuNgay AND @denNgay
        GROUP BY pn.idNhaCungCap;
    END
    ELSE IF @loaiDoiTuong = 'KHACH'
    BEGIN
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + px.idKhachHang,
            @maPhieuTinh,
            px.idKhachHang,
            SUM(ct.soLuong * 500), -- Giả định logic cố định
            'Pending'
        FROM PhieuXuat px
        JOIN ChiTietPhieuXuat ct ON px.maPhieu = ct.maPhieu
        WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay
        GROUP BY px.idKhachHang;
    END
END;
GO

-- 4. Method: QuanLyTaiChinh.tinhLoiNhuanThuc() (P&L Report)
-- Mô tả: Tính LN ròng theo công thức: DT - GV + CK_NCC - CK_Khach - CPVH [Source 12, 20, 30]
IF OBJECT_ID('sp_TaiChinh_BaoCaoLoiNhuan', 'P') IS NOT NULL DROP PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan;
GO
CREATE PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan
    @tuNgay DATE, @denNgay DATE
AS
BEGIN
    -- Doanh thu
    DECLARE @DoanhThu DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM PhieuXuat WHERE ngayXuat BETWEEN @tuNgay AND @denNgay);
    -- Giá vốn (lấy snapshot)
    DECLARE @GiaVon DECIMAL(18,2) = (SELECT ISNULL(SUM(soLuong * giaVonTaiThoiDiem),0) 
                                     FROM ChiTietPhieuXuat ctp JOIN PhieuXuat px ON ctp.maPhieu = px.maPhieu 
                                     WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay);
    -- CK NCC (Thu nhập)
    DECLARE @CK_NCC DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM BangKeChietKhau bk 
                                     JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                     WHERE pt.loaiDoiTuong = 'NCC' AND bk.trangThai = 'Approved');
    -- CK Khách (Chi phí)
    DECLARE @CK_Khach DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM BangKeChietKhau bk 
                                       JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                       WHERE pt.loaiDoiTuong = 'KHACH' AND bk.trangThai = 'Approved');
    -- Chi phí vận hành
    DECLARE @ChiPhiVH DECIMAL(18,2) = (SELECT ISNULL(SUM(soTien),0) FROM PhieuThuChi WHERE loaiPhieu = 'CHI' AND lyDo LIKE N'%Vận hành%');

    SELECT @DoanhThu AS DoanhThu, @GiaVon AS GiaVon, @CK_NCC AS ThuTuCK, @CK_Khach AS ChiPhiCK, @ChiPhiVH AS ChiPhiVH,
           (@DoanhThu - @GiaVon + @CK_NCC - @CK_Khach - @ChiPhiVH) AS LoiNhuanRong;
END;
GO

-- ====================================================================================
-- PHẦN B: CÁC STORED PROCEDURE CRUD (Thêm, Sửa, Xóa, Lấy dữ liệu)
-- ====================================================================================

-- 1. CRUD HANGHOA
IF OBJECT_ID('sp_HangHoa_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_Insert;
GO
CREATE PROCEDURE sp_HangHoa_Insert @maHang VARCHAR(20), @tenHang NVARCHAR(100), @donViTinh NVARCHAR(20), @quyCach NVARCHAR(50) AS
INSERT INTO HangHoa(maHang, tenHang, donViTinh, quyCach) VALUES (@maHang, @tenHang, @donViTinh, @quyCach);
GO

IF OBJECT_ID('sp_HangHoa_Update', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_Update;
GO
CREATE PROCEDURE sp_HangHoa_Update @maHang VARCHAR(20), @tenHang NVARCHAR(100), @donViTinh NVARCHAR(20), @quyCach NVARCHAR(50) AS
UPDATE HangHoa SET tenHang=@tenHang, donViTinh=@donViTinh, quyCach=@quyCach WHERE maHang=@maHang;
GO

IF OBJECT_ID('sp_HangHoa_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_Delete;
GO
CREATE PROCEDURE sp_HangHoa_Delete @maHang VARCHAR(20) AS DELETE FROM HangHoa WHERE maHang=@maHang;
GO

IF OBJECT_ID('sp_HangHoa_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_SelectAll;
GO
CREATE PROCEDURE sp_HangHoa_SelectAll AS SELECT * FROM HangHoa;
GO

IF OBJECT_ID('sp_HangHoa_SelectById', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_SelectById;
GO
CREATE PROCEDURE sp_HangHoa_SelectById @maHang VARCHAR(20) AS SELECT * FROM HangHoa WHERE maHang=@maHang;
GO

-- 2. CRUD NHACUNGCAP
IF OBJECT_ID('sp_NhaCungCap_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_Insert;
GO
CREATE PROCEDURE sp_NhaCungCap_Insert @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @maSoThue VARCHAR(50), @soNgayDuocNo INT AS
INSERT INTO NhaCungCap(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo) VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @maSoThue, @soNgayDuocNo);
GO

IF OBJECT_ID('sp_NhaCungCap_Update', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_Update;
GO
CREATE PROCEDURE sp_NhaCungCap_Update @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @maSoThue VARCHAR(50), @soNgayDuocNo INT AS
UPDATE NhaCungCap SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, maSoThue=@maSoThue, soNgayDuocNo=@soNgayDuocNo WHERE maDoiTuong=@maDoiTuong;
GO

IF OBJECT_ID('sp_NhaCungCap_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_Delete;
GO
CREATE PROCEDURE sp_NhaCungCap_Delete @maDoiTuong VARCHAR(20) AS DELETE FROM NhaCungCap WHERE maDoiTuong=@maDoiTuong;
GO

IF OBJECT_ID('sp_NhaCungCap_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_SelectAll;
GO
CREATE PROCEDURE sp_NhaCungCap_SelectAll AS SELECT * FROM NhaCungCap;
GO

-- 3. CRUD KHACHHANG
IF OBJECT_ID('sp_KhachHang_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_KhachHang_Insert;
GO
CREATE PROCEDURE sp_KhachHang_Insert @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @aoNuoi NVARCHAR(100) AS
INSERT INTO KhachHang(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi) VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @aoNuoi);
GO

IF OBJECT_ID('sp_KhachHang_Update', 'P') IS NOT NULL DROP PROCEDURE sp_KhachHang_Update;
GO
CREATE PROCEDURE sp_KhachHang_Update @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @aoNuoi NVARCHAR(100) AS
UPDATE KhachHang SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, aoNuoi=@aoNuoi WHERE maDoiTuong=@maDoiTuong;
GO

IF OBJECT_ID('sp_KhachHang_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_KhachHang_SelectAll;
GO
CREATE PROCEDURE sp_KhachHang_SelectAll AS SELECT * FROM KhachHang;
GO

-- 4. CRUD KHO & CHITIETTON
IF OBJECT_ID('sp_Kho_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_Kho_Insert;
GO
CREATE PROCEDURE sp_Kho_Insert @maKho VARCHAR(20), @tenKho NVARCHAR(100), @loaiKho VARCHAR(20), @maDaiLyPhuTrach VARCHAR(20) AS
INSERT INTO Kho(maKho, tenKho, loaiKho, maDaiLyPhuTrach) VALUES (@maKho, @tenKho, @loaiKho, @maDaiLyPhuTrach);
GO

IF OBJECT_ID('sp_ChiTietTon_Upsert', 'P') IS NOT NULL DROP PROCEDURE sp_ChiTietTon_Upsert;
GO
CREATE PROCEDURE sp_ChiTietTon_Upsert @maKho VARCHAR(20), @maHang VARCHAR(20), @soLuongTon FLOAT, @giaTriTon DECIMAL(18,2) AS
BEGIN
    IF EXISTS (SELECT 1 FROM ChiTietTon WHERE maKho = @maKho AND maHang = @maHang)
        UPDATE ChiTietTon SET soLuongTon = @soLuongTon, giaTriTon = @giaTriTon WHERE maKho = @maKho AND maHang = @maHang
    ELSE
        INSERT INTO ChiTietTon(maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKho, @maHang, @soLuongTon, @giaTriTon)
END;
GO

IF OBJECT_ID('sp_ChiTietTon_SelectByKho', 'P') IS NOT NULL DROP PROCEDURE sp_ChiTietTon_SelectByKho;
GO
CREATE PROCEDURE sp_ChiTietTon_SelectByKho @maKho VARCHAR(20) AS SELECT * FROM ChiTietTon WHERE maKho = @maKho;
GO

-- 5. CRUD PHIEUNHAP & DETAILS
IF OBJECT_ID('sp_PhieuNhap_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuNhap_Insert;
GO
CREATE PROCEDURE sp_PhieuNhap_Insert @maPhieu VARCHAR(20), @ngayNhap DATETIME, @idDaiLyNhap VARCHAR(20), @idNhaCungCap VARCHAR(20) AS
INSERT INTO PhieuNhap(maPhieu, ngayNhap, idDaiLyNhap, idNhaCungCap, tongTien) VALUES (@maPhieu, @ngayNhap, @idDaiLyNhap, @idNhaCungCap, 0);
GO

IF OBJECT_ID('sp_ChiTietPhieuNhap_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_ChiTietPhieuNhap_Insert;
GO
CREATE PROCEDURE sp_ChiTietPhieuNhap_Insert @maPhieu VARCHAR(20), @maHang VARCHAR(20), @soLuong FLOAT, @donGiaNhap DECIMAL(18,2) AS
INSERT INTO ChiTietPhieuNhap(maPhieu, maHang, soLuong, donGiaNhap) VALUES (@maPhieu, @maHang, @soLuong, @donGiaNhap);
GO

IF OBJECT_ID('sp_PhieuNhap_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuNhap_Delete;
GO
CREATE PROCEDURE sp_PhieuNhap_Delete @maPhieu VARCHAR(20) AS
BEGIN
    DELETE FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu;
    DELETE FROM PhieuNhap WHERE maPhieu = @maPhieu;
END;
GO

IF OBJECT_ID('sp_PhieuNhap_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuNhap_SelectAll;
GO
CREATE PROCEDURE sp_PhieuNhap_SelectAll AS SELECT * FROM PhieuNhap;
GO

-- 6. CRUD PHIEUXUAT & DETAILS
IF OBJECT_ID('sp_PhieuXuat_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuXuat_Insert;
GO
CREATE PROCEDURE sp_PhieuXuat_Insert @maPhieu VARCHAR(20), @ngayXuat DATETIME, @idDaiLyBan VARCHAR(20), @idKhachHang VARCHAR(20) AS
INSERT INTO PhieuXuat(maPhieu, ngayXuat, idDaiLyBan, idKhachHang, tongTien) VALUES (@maPhieu, @ngayXuat, @idDaiLyBan, @idKhachHang, 0);
GO

IF OBJECT_ID('sp_ChiTietPhieuXuat_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_ChiTietPhieuXuat_Insert;
GO
CREATE PROCEDURE sp_ChiTietPhieuXuat_Insert @maPhieu VARCHAR(20), @maHang VARCHAR(20), @soLuong FLOAT, @giaBan DECIMAL(18,2) AS
INSERT INTO ChiTietPhieuXuat(maPhieu, maHang, soLuong, giaBan, giaVonTaiThoiDiem) VALUES (@maPhieu, @maHang, @soLuong, @giaBan, 0);
GO

IF OBJECT_ID('sp_PhieuXuat_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuXuat_Delete;
GO
CREATE PROCEDURE sp_PhieuXuat_Delete @maPhieu VARCHAR(20) AS
BEGIN
    DELETE FROM ChiTietPhieuXuat WHERE maPhieu = @maPhieu;
    DELETE FROM PhieuXuat WHERE maPhieu = @maPhieu;
END;
GO

-- 7. CRUD CHIETKHAU & SORIENG
IF OBJECT_ID('sp_CauHinhChietKhau_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_CauHinhChietKhau_Insert;
GO
CREATE PROCEDURE sp_CauHinhChietKhau_Insert @tenCauHinh NVARCHAR(100), @loaiQuyLuat VARCHAR(20), @doiTuongApDung VARCHAR(20), @chiTietLuat NVARCHAR(MAX) AS
INSERT INTO CauHinhChietKhau(tenCauHinh, loaiQuyLuat, doiTuongApDung, chiTietLuat) VALUES (@tenCauHinh, @loaiQuyLuat, @doiTuongApDung, @chiTietLuat);
GO

IF OBJECT_ID('sp_BangKeChietKhau_SelectByPhieuTinh', 'P') IS NOT NULL DROP PROCEDURE sp_BangKeChietKhau_SelectByPhieuTinh;
GO
CREATE PROCEDURE sp_BangKeChietKhau_SelectByPhieuTinh @maPhieuTinh VARCHAR(20) AS SELECT * FROM BangKeChietKhau WHERE maPhieuTinh = @maPhieuTinh;
GO

IF OBJECT_ID('sp_SoRiengKhachHang_SelectByKhachHang', 'P') IS NOT NULL DROP PROCEDURE sp_SoRiengKhachHang_SelectByKhachHang;
GO
CREATE PROCEDURE sp_SoRiengKhachHang_SelectByKhachHang @maKhachHang VARCHAR(20) AS SELECT * FROM SoRiengKhachHang WHERE maKhachHang = @maKhachHang ORDER BY ngayGiaoDich DESC;
GO