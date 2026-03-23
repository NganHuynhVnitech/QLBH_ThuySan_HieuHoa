-- =============================================
-- DATABASE SETUP: Cá»¬A HÃ€NG THá»¦Y Sáº¢N HIá»†U HOA
-- Dá»±a trÃªn Class Diagram Docx [1-10]
-- =============================================


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
IF OBJECT_ID('ChiTietPhieuTinh', 'U') IS NOT NULL DROP TABLE ChiTietPhieuTinh;
IF OBJECT_ID('PhieuTinhChietKhau', 'U') IS NOT NULL DROP TABLE PhieuTinhChietKhau;
IF OBJECT_ID('CauHinhChietKhau', 'U') IS NOT NULL DROP TABLE CauHinhChietKhau;
IF OBJECT_ID('SoRiengKhachHang', 'U') IS NOT NULL DROP TABLE SoRiengKhachHang;
IF OBJECT_ID('PhieuThuChi', 'U') IS NOT NULL DROP TABLE PhieuThuChi;
IF OBJECT_ID('HangHoa', 'U') IS NOT NULL DROP TABLE HangHoa;
IF OBJECT_ID('DonViTinh', 'U') IS NOT NULL DROP TABLE DonViTinh;
IF OBJECT_ID('DaiLy', 'U') IS NOT NULL DROP TABLE DaiLy;
IF OBJECT_ID('KhachHang', 'U') IS NOT NULL DROP TABLE KhachHang;
IF OBJECT_ID('NhaCungCap', 'U') IS NOT NULL DROP TABLE NhaCungCap;
IF OBJECT_ID('DoiTuongChiPhi', 'U') IS NOT NULL DROP TABLE DoiTuongChiPhi;

-- =============================================
-- 2. CREATE TABLES (MASTER DATA) [1, 3]
-- =============================================

-- Class: HangHoa [1]
CREATE TABLE HangHoa (
    maHang VARCHAR(20) PRIMARY KEY,
    tenHang NVARCHAR(100) NOT NULL,
    donViTinh NVARCHAR(20),
    quyCach NVARCHAR(50),
    giaVonHienTai DECIMAL(18, 2) DEFAULT 0, -- Cáº­p nháº­t bá»Ÿi COGS Engine
    giaBanHienTai DECIMAL(18, 2) DEFAULT 0,
    isDisabled BIT NOT NULL DEFAULT 0
);

-- Class: DonViTinh (Unit Conversion) [New User Request]
CREATE TABLE DonViTinh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    maHang VARCHAR(20),
    tenDonVi NVARCHAR(50),
    tyLeQuyDoi INT DEFAULT 1,
    giaBan DECIMAL(18, 2) DEFAULT 0,
    maHangDonVi VARCHAR(50), -- MÃ£ quy Ä‘á»•i (vÃ­ dá»¥ SP001-LOC)
    FOREIGN KEY (maHang) REFERENCES HangHoa(maHang) ON DELETE CASCADE
);

-- Class: NhaCungCap (Inherits DoiTuong) [3]
CREATE TABLE NhaCungCap (
    maDoiTuong VARCHAR(20) PRIMARY KEY,
    tenDoiTuong NVARCHAR(100),
    soDienThoai VARCHAR(20),
    diaChi NVARCHAR(200),
    maSoThue VARCHAR(50),
    soNgayDuocNo INT DEFAULT 0, -- Quan trá»ng Ä‘á»ƒ tÃ­nh háº¡n thanh toÃ¡n
    duNoLuyKe DECIMAL(18, 2) DEFAULT 0, -- Tá»•ng dÆ° ná»£ hiá»‡n táº¡i
    isDisabled BIT NOT NULL DEFAULT 0
);

-- Class: KhachHang (Inherits DoiTuong) [3]
CREATE TABLE KhachHang (
    maDoiTuong VARCHAR(20) PRIMARY KEY,
    tenDoiTuong NVARCHAR(100),
    soDienThoai VARCHAR(20),
    diaChi NVARCHAR(200),
    aoNuoi NVARCHAR(100),
    duNoLuyKe DECIMAL(18, 2) DEFAULT 0, -- LiÃªn káº¿t SoRieng
    isDisabled BIT NOT NULL DEFAULT 0
);

-- Class: DaiLy (Inherits DoiTuong) [3]
CREATE TABLE DaiLy (
    maDaiLy VARCHAR(20) PRIMARY KEY, -- Mapping maDoiTuong
    tenDaiLy NVARCHAR(100),
    loaiDaiLy VARCHAR(20) CHECK (loaiDaiLy IN ('NHAP_A', 'NHAP_B', 'BAN_C'))
);

-- Class: DoiTuongChiPhi (Ex: Tien Dien, Nuoc, Luong)
CREATE TABLE DoiTuongChiPhi (
    maDoiTuong VARCHAR(20) PRIMARY KEY,
    tenDoiTuong NVARCHAR(100) NOT NULL
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
    giaTriTon DECIMAL(18, 2) DEFAULT 0, -- Chá»‰ dÃ¹ng cho Kho Tá»•ng áº¢o
    PRIMARY KEY (maKho, maHang),
    FOREIGN KEY (maKho) REFERENCES Kho(maKho),
    FOREIGN KEY (maHang) REFERENCES HangHoa(maHang)
);

-- =============================================
-- 4. OPERATIONS (NHáº¬P - XUáº¤T) [6, 7]
-- =============================================

-- Class: PhieuNhap [6]
CREATE TABLE PhieuNhap (
    maPhieu VARCHAR(20) PRIMARY KEY,
    ngayNhap DATETIME DEFAULT GETDATE(),
    idDaiLyNhap VARCHAR(20),
    idNhaCungCap VARCHAR(20),
    hanThanhToan DATETIME, -- TÃ­nh toÃ¡n: ngayNhap + soNgayDuocNo
    tongTien DECIMAL(18, 2) DEFAULT 0,
    ngayThanhToan DATETIME NULL, -- [NEW] Payment Date
    trangthaithanhtoan NVARCHAR(50) DEFAULT N'Chưa Thanh Toán',
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
    ngayThanhToan DATETIME NULL, -- [NEW] Payment Date
    trangthaithanhtoan NVARCHAR(50) DEFAULT N'Chưa Thanh Toán',
    FOREIGN KEY (idDaiLyBan) REFERENCES DaiLy(maDaiLy),
    FOREIGN KEY (idKhachHang) REFERENCES KhachHang(maDoiTuong)
);

-- Class: ChiTietPhieuXuat [7]
CREATE TABLE ChiTietPhieuXuat (
    maPhieu VARCHAR(20),
    maHang VARCHAR(20),
    soLuong FLOAT,
    giaBan DECIMAL(18, 2),
    giaVonTaiThoiDiem DECIMAL(18, 2), -- Snapshot giÃ¡ vá»‘n lÃºc xuáº¥t
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
    -- JSON hoáº·c XML lÆ°u chi tiáº¿t cÃ¡c má»‘c (List<MocSanLuong> trong diagram)
    chiTietLuat NVARCHAR(MAX) 
);

-- Class: PhieuTinhChietKhau [8]
CREATE TABLE PhieuTinhChietKhau (
    maPhieuTinh VARCHAR(20) PRIMARY KEY,
    loaiDoiTuong VARCHAR(10) CHECK (loaiDoiTuong IN ('NCC', 'KHACH')),
    maDoiTuong VARCHAR(20), -- Link specific NCC or Customer
    tuNgay DATE,
    denNgay DATE,
    ngayTao DATETIME DEFAULT GETDATE(),
    tongTien DECIMAL(18, 2) DEFAULT 0,
    trangThai NVARCHAR(50) DEFAULT N'ChÆ°a thanh toÃ¡n', -- ChÆ°a thanh toÃ¡n / ÄÃ£ thanh toÃ¡n
    ngayThanhToan DATETIME NULL
);

-- Class: ChiTietPhieuTinh (Replaces BangKeChietKhau for detail storage)
CREATE TABLE ChiTietPhieuTinh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    maPhieuTinh VARCHAR(20),
    maHang VARCHAR(20),
    soLuong FLOAT, -- Tá»•ng sá»‘ lÆ°á»£ng mua/bÃ¡n trong ká»³
    soTienChietKhau DECIMAL(18, 2), -- ÄÆ¡n giÃ¡ chiáº¿t kháº¥u hoáº·c Tá»•ng tiá»n chiáº¿t kháº¥u tÃ¹y logic
    thanhTien DECIMAL(18, 2), -- ThÃ nh tiá»n cuá»‘i cÃ¹ng cho dÃ²ng nÃ y
    noiDung NVARCHAR(200), -- Diá»…n giáº£i (VD: Báº­c 1 (>100kg))
    FOREIGN KEY (maPhieuTinh) REFERENCES PhieuTinhChietKhau(maPhieuTinh) ON DELETE CASCADE,
    FOREIGN KEY (maHang) REFERENCES HangHoa(maHang)
);

-- Class: SoRiengKhachHang (LÆ°u lá»‹ch sá»­ giao dá»‹ch) [10]
CREATE TABLE SoRiengKhachHang (
    id INT IDENTITY(1,1) PRIMARY KEY,
    maKhachHang VARCHAR(20),
    ngayGiaoDich DATETIME,
    loaiGiaoDich VARCHAR(20) CHECK (loaiGiaoDich IN ('MUA_HANG', 'THANH_TOAN', 'CAN_TRU')),
    soTienPhatSinh DECIMAL(18, 2), -- DÆ°Æ¡ng: TÄƒng ná»£, Ã‚m: Giáº£m ná»£
    dienGiai NVARCHAR(200),
    FOREIGN KEY (maKhachHang) REFERENCES KhachHang(maDoiTuong)
);

-- Class: PhieuThuChi [2]
CREATE TABLE PhieuThuChi (
    maPhieu VARCHAR(20) PRIMARY KEY,
    loaiPhieu VARCHAR(10) CHECK (loaiPhieu IN ('THU', 'CHI', 'NHAP')),
    ngayLap DATETIME DEFAULT GETDATE(),
    soTien DECIMAL(18, 2),
    lyDo NVARCHAR(200), -- Thu bán hàng, Chi trả NCC...
    maDoiTuong VARCHAR(20) NULL -- Linked to KhachHang, NhaCungCap, or DoiTuongChiPhi
);

GO

-- =============================================
-- 6. STORED PROCEDURES (MAPPING CLASS METHODS)
-- =============================================

-- Method: KhoTongAo.tinhGiaVonBinhQuanLienHoan() [4]
-- Method: PhieuNhap.triggerDongBoKho() [6]
-- Logic: Cáº­p nháº­t tá»“n kho váº­t lÃ½ + kho tá»•ng + tÃ­nh láº¡i giÃ¡ vá»‘n
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

    SELECT @idDaiLy = idDaiLyNhap FROM PhieuNhap WHERE maPhieu = @maPhieu;
    -- Giáº£ Ä‘á»‹nh mapping: ID Äáº¡i lÃ½ = ID Kho Váº­t lÃ½
    SET @maKhoVatLy = @idDaiLy; 

    -- 1. Cáº­p nháº­t Kho Váº­t LÃ½ (Chá»‰ tÄƒng sá»‘ lÆ°á»£ng)
    MERGE INTO ChiTietTon AS Target
    USING (SELECT maHang, soLuong FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu) AS Source
    ON Target.maKho = @maKhoVatLy AND Target.maHang = Source.maHang
    WHEN MATCHED THEN
        UPDATE SET soLuongTon = soLuongTon + Source.soLuong
    WHEN NOT MATCHED THEN
        INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoVatLy, Source.maHang, Source.soLuong, 0);

    -- 2. Cáº­p nháº­t Kho Tá»•ng & TÃ­nh GiÃ¡ Vá»‘n (BÃ¬nh quÃ¢n gia quyá»n liÃªn hoÃ n)
    -- CÃ´ng thá»©c: GiÃ¡ Má»›i = (GiÃ¡ Trá»‹ CÅ© + GiÃ¡ Trá»‹ Nháº­p Má»›i) / (SL CÅ© + SL Nháº­p Má»›i)
    
    DECLARE cur CURSOR FOR SELECT maHang, soLuong, donGiaNhap FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu;
    DECLARE @maHang VARCHAR(20), @slNhap FLOAT, @giaNhap DECIMAL(18,2);
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Láº¥y dá»¯ liá»‡u cÅ© tá»« Kho Tá»•ng
        DECLARE @slCu FLOAT = 0;
        DECLARE @giaVonCu DECIMAL(18,2) = 0;
        
        SELECT @slCu = soLuongTon, @giaVonCu = giaTriTon 
        FROM ChiTietTon WHERE maKho = @maKhoTong AND maHang = @maHang;

        IF @slCu IS NULL SET @slCu = 0;
        IF @giaVonCu IS NULL SET @giaVonCu = 0;

        -- TÃ­nh toÃ¡n
        DECLARE @tongGiaTriMoi DECIMAL(18,2) = (@slCu * @giaVonCu) + (@slNhap * @giaNhap);
        DECLARE @tongSlMoi FLOAT = @slCu + @slNhap;
        DECLARE @giaVonMoi DECIMAL(18,2) = 0;

        IF @tongSlMoi > 0 
            SET @giaVonMoi = @tongGiaTriMoi / @tongSlMoi;
        
        -- Cáº­p nháº­t Kho Tá»•ng
        MERGE INTO ChiTietTon AS Target
        USING (SELECT @maHang AS maHang) AS Source
        ON Target.maKho = @maKhoTong AND Target.maHang = Source.maHang
        WHEN MATCHED THEN
            UPDATE SET soLuongTon = @tongSlMoi, giaTriTon = @giaVonMoi
        WHEN NOT MATCHED THEN
            INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoTong, @maHang, @tongSlMoi, @giaVonMoi);
            
        -- Cáº­p nháº­t láº¡i giÃ¡ vá»‘n vÃ o Master HÃ ng HÃ³a Ä‘á»ƒ query nhanh
        UPDATE HangHoa SET giaVonHienTai = @giaVonMoi WHERE maHang = @maHang;

        FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END;
GO

-- Method: PhieuNhap.tinhHanThanhToan() [6]
-- Trigger tá»± Ä‘á»™ng tÃ­nh háº¡n thanh toÃ¡n khi Insert PhieuNhap
IF OBJECT_ID('trg_PhieuNhap_TinhHanThanhToan', 'TR') IS NOT NULL DROP TRIGGER trg_PhieuNhap_TinhHanThanhToan;
GO
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
IF OBJECT_ID('sp_PhieuXuat_XuLy', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuXuat_XuLy;
GO
CREATE PROCEDURE sp_PhieuXuat_XuLy
    @maPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Snapshot GiÃ¡ Vá»‘n & Update Chi Tiáº¿t Phiáº¿u Xuáº¥t
    UPDATE ctp
    SET giaVonTaiThoiDiem = hh.giaVonHienTai
    FROM ChiTietPhieuXuat ctp
    JOIN HangHoa hh ON ctp.maHang = hh.maHang
    WHERE ctp.maPhieu = @maPhieu;

    -- 2. Giáº£m Tá»“n Kho Váº­t LÃ½ & Kho Tá»•ng
    DECLARE @idDaiLy VARCHAR(20), @idKhachHang VARCHAR(20), @ngayXuat DATETIME, @tongTien DECIMAL(18,2);
    SELECT @idDaiLy = idDaiLyBan, @idKhachHang = idKhachHang, @ngayXuat = ngayXuat FROM PhieuXuat WHERE maPhieu = @maPhieu;

    -- Update Kho (Logic giáº£n lÆ°á»£c: Trá»« tháº³ng)
    UPDATE t
    SET t.soLuongTon = t.soLuongTon - ctp.soLuong
    FROM ChiTietTon t
    JOIN ChiTietPhieuXuat ctp ON t.maHang = ctp.maHang
    WHERE ctp.maPhieu = @maPhieu AND t.maKho IN (@idDaiLy, 'KHO_TONG_AO');

    -- 3. Ghi Ná»£ Sá»• RiÃªng
    SELECT @tongTien = SUM(thanhTien) FROM ChiTietPhieuXuat WHERE maPhieu = @maPhieu;
    
    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai)
    VALUES (@idKhachHang, @ngayXuat, 'MUA_HANG', @tongTien, N'PhÃ¡t sinh tá»« phiáº¿u xuáº¥t ' + @maPhieu);

    -- Update DÆ° ná»£ lÅ©y káº¿ master
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe + @tongTien WHERE maDoiTuong = @idKhachHang;
    
    -- Update Tong tien Header
    UPDATE PhieuXuat SET tongTien = @tongTien WHERE maPhieu = @maPhieu;
END;
GO

-- Method: PhieuTinhChietKhau.tinhToan() [8]
-- Engine TÃ­nh toÃ¡n chiáº¿t kháº¥u (Ráº¥t phá»©c táº¡p, Ä‘Ã¢y lÃ  khung sÆ°á»n Logic)
IF OBJECT_ID('sp_Engine_TinhChietKhau', 'P') IS NOT NULL DROP PROCEDURE sp_Engine_TinhChietKhau;
GO
CREATE PROCEDURE sp_Engine_TinhChietKhau
    @maPhieuTinh VARCHAR(20)
AS
BEGIN
    DECLARE @loaiDoiTuong VARCHAR(10), @tuNgay DATE, @denNgay DATE;
    SELECT @loaiDoiTuong = loaiDoiTuong, @tuNgay = tuNgay, @denNgay = denNgay 
    FROM PhieuTinhChietKhau WHERE maPhieuTinh = @maPhieuTinh;

    IF @loaiDoiTuong = 'NCC'
    BEGIN
        -- Logic: QuÃ©t PhieuNhap -> Group by NCC, HangHoa -> Sum(SoLuong)
        -- Ãp dá»¥ng CauHinhChietKhau (Giáº£ sá»­ Ã¡p dá»¥ng Fixed Rule cho demo)
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + pn.idNhaCungCap,
            @maPhieuTinh,
            pn.idNhaCungCap,
            SUM(ct.soLuong * 1000), -- VÃ­ dá»¥: 1000Ä‘/kg (Cáº§n join vá»›i CauHinhChietKhau Ä‘á»ƒ láº¥y sá»‘ thá»±c)
            'Pending'
        FROM PhieuNhap pn
        JOIN ChiTietPhieuNhap ct ON pn.maPhieu = ct.maPhieu
        WHERE pn.ngayNhap BETWEEN @tuNgay AND @denNgay
        GROUP BY pn.idNhaCungCap;
    END
    ELSE IF @loaiDoiTuong = 'KHACH'
    BEGIN
        -- Logic tÆ°Æ¡ng tá»± cho KhÃ¡ch hÃ ng
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + px.idKhachHang,
            @maPhieuTinh,
            px.idKhachHang,
            SUM(ct.soLuong * 500), -- VÃ­ dá»¥: 500Ä‘/kg
            'Pending'
        FROM PhieuXuat px
        JOIN ChiTietPhieuXuat ct ON px.maPhieu = ct.maPhieu
        WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay
        GROUP BY px.idKhachHang;
    END
END;
GO

-- Method: QuanLyTaiChinh.canhBaoNhacNo() [2]
IF OBJECT_ID('sp_TaiChinh_CanhBaoNhacNo', 'P') IS NOT NULL DROP PROCEDURE sp_TaiChinh_CanhBaoNhacNo;
GO
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
    WHERE pn.hanThanhToan <= DATEADD(day, 3, GETDATE()) -- Cáº£nh bÃ¡o trÆ°á»›c 3 ngÃ y hoáº·c Ä‘Ã£ quÃ¡ háº¡n
    ORDER BY pn.hanThanhToan ASC;
END;
GO

-- Method: QuanLyTaiChinh.tinhLoiNhuanThuc() [2]
-- Output: P&L Report
IF OBJECT_ID('sp_TaiChinh_BaoCaoLoiNhuan', 'P') IS NOT NULL DROP PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan;
GO
CREATE PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan
    @tuNgay DATE,
    @denNgay DATE
AS
BEGIN
    -- 1. Doanh Thu
    DECLARE @DoanhThu DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM PhieuXuat WHERE ngayXuat BETWEEN @tuNgay AND @denNgay);
    
    -- 2. GiÃ¡ Vá»‘n (Láº¥y tá»« snapshot lÃºc xuáº¥t)
    DECLARE @GiaVon DECIMAL(18,2) = (SELECT ISNULL(SUM(soLuong * giaVonTaiThoiDiem),0) 
                                     FROM ChiTietPhieuXuat ctp 
                                     JOIN PhieuXuat px ON ctp.maPhieu = px.maPhieu 
                                     WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay);
    
    -- 3. Chiáº¿t kháº¥u NCC (Thu nháº­p khÃ¡c)
    DECLARE @CK_NCC DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) 
                                     FROM BangKeChietKhau bk 
                                     JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                     WHERE pt.loaiDoiTuong = 'NCC' AND bk.trangThai = 'Approved');

    -- 4. Chiáº¿t kháº¥u KhÃ¡ch (Chi phÃ­ bÃ¡n hÃ ng)
    DECLARE @CK_Khach DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) 
                                       FROM BangKeChietKhau bk 
                                       JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                       WHERE pt.loaiDoiTuong = 'KHACH' AND bk.trangThai = 'Approved');

    -- 5. Chi phÃ­ váº­n hÃ nh
    DECLARE @ChiPhiVH DECIMAL(18,2) = (SELECT ISNULL(SUM(soTien),0) FROM PhieuThuChi WHERE loaiPhieu = 'CHI' AND lyDo LIKE N'%Váº­n hÃ nh%');

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
-- PHáº¦N A: CÃC STORED PROCEDURE Xá»¬ LÃ NGHIá»†P Vá»¤ (BUSINESS LOGIC)
-- (TÆ°Æ¡ng á»©ng vá»›i cÃ¡c Method trong Class Diagram: tÃ­nh giÃ¡ vá»‘n, chiáº¿t kháº¥u, sá»• riÃªng...)
-- ====================================================================================

-- 1. Method: KhoTongAo.tinhGiaVonBinhQuanLienHoan() & triggerDongBoKho()
-- MÃ´ táº£: Cáº­p nháº­t tá»“n kho váº­t lÃ½, Ä‘á»“ng bá»™ sang kho tá»•ng vÃ  tÃ­nh láº¡i giÃ¡ vá»‘n ngay láº­p tá»©c [Source 2, 6, 15, 23, 24]
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

    -- Láº¥y ID Äáº¡i lÃ½ nháº­p Ä‘á»ƒ xÃ¡c Ä‘á»‹nh kho váº­t lÃ½
    SELECT @idDaiLy = idDaiLyNhap FROM PhieuNhap WHERE maPhieu = @maPhieu;
    SET @maKhoVatLy = @idDaiLy; -- Giáº£ Ä‘á»‹nh ID Äáº¡i lÃ½ map vá»›i ID Kho

    -- BÆ¯á»šC 1: Cáº­p nháº­t Kho Váº­t LÃ½ (Chá»‰ tÄƒng sá»‘ lÆ°á»£ng thá»±c táº¿)
    MERGE INTO ChiTietTon AS Target
    USING (SELECT maHang, soLuong FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu) AS Source
    ON Target.maKho = @maKhoVatLy AND Target.maHang = Source.maHang
    WHEN MATCHED THEN
        UPDATE SET soLuongTon = soLuongTon + Source.soLuong
    WHEN NOT MATCHED THEN
        INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoVatLy, Source.maHang, Source.soLuong, 0);

    -- BÆ¯á»šC 2: TÃ­nh toÃ¡n COGS (BÃ¬nh quÃ¢n gia quyá»n liÃªn hoÃ n) táº¡i Kho Tá»•ng
    DECLARE cur CURSOR FOR SELECT maHang, soLuong, donGiaNhap FROM ChiTietPhieuNhap WHERE maPhieu = @maPhieu;
    DECLARE @maHang VARCHAR(20), @slNhap FLOAT, @giaNhap DECIMAL(18,2);
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @slCu FLOAT = 0;
        DECLARE @giaVonCu DECIMAL(18,2) = 0;
        
        -- Láº¥y dá»¯ liá»‡u cÅ© tá»« Kho Tá»•ng
        SELECT @slCu = soLuongTon, @giaVonCu = giaTriTon 
        FROM ChiTietTon WHERE maKho = @maKhoTong AND maHang = @maHang;

        SET @slCu = ISNULL(@slCu, 0);
        SET @giaVonCu = ISNULL(@giaVonCu, 0);

        -- CÃ´ng thá»©c Moving Weighted Average [Source 5]
        DECLARE @tongGiaTriMoi DECIMAL(18,2) = (@slCu * @giaVonCu) + (@slNhap * @giaNhap);
        DECLARE @tongSlMoi FLOAT = @slCu + @slNhap;
        DECLARE @giaVonMoi DECIMAL(18,2) = 0;

        IF @tongSlMoi > 0 SET @giaVonMoi = @tongGiaTriMoi / @tongSlMoi;
        
        -- Cáº­p nháº­t láº¡i Kho Tá»•ng
        MERGE INTO ChiTietTon AS Target
        USING (SELECT @maHang AS maHang) AS Source
        ON Target.maKho = @maKhoTong AND Target.maHang = Source.maHang
        WHEN MATCHED THEN
            UPDATE SET soLuongTon = @tongSlMoi, giaTriTon = @giaVonMoi
        WHEN NOT MATCHED THEN
            INSERT (maKho, maHang, soLuongTon, giaTriTon) VALUES (@maKhoTong, @maHang, @tongSlMoi, @giaVonMoi);
            
        -- Cáº­p nháº­t giÃ¡ vá»‘n master
        UPDATE HangHoa SET giaVonHienTai = @giaVonMoi WHERE maHang = @maHang;

        FETCH NEXT FROM cur INTO @maHang, @slNhap, @giaNhap;
    END
    CLOSE cur; DEALLOCATE cur;
END;
GO

-- 2. Method: PhieuXuat.ghiNoSoRieng() & Kho.giamTon()
-- MÃ´ táº£: Xá»­ lÃ½ phiáº¿u xuáº¥t, snapshot giÃ¡ vá»‘n, trá»« kho vÃ  ghi ná»£ sá»• riÃªng [Source 6, 16, 26, 29]
IF OBJECT_ID('sp_PhieuXuat_XuLy', 'P') IS NOT NULL DROP PROCEDURE sp_PhieuXuat_XuLy;
GO
CREATE PROCEDURE sp_PhieuXuat_XuLy
    @maPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Snapshot GiÃ¡ Vá»‘n (LÆ°u giÃ¡ vá»‘n táº¡i thá»i Ä‘iá»ƒm xuáº¥t Ä‘á»ƒ tÃ­nh lÃ£i lá»— chÃ­nh xÃ¡c)
    UPDATE ctp
    SET giaVonTaiThoiDiem = hh.giaVonHienTai
    FROM ChiTietPhieuXuat ctp
    JOIN HangHoa hh ON ctp.maHang = hh.maHang
    WHERE ctp.maPhieu = @maPhieu;

    -- 2. Giáº£m Tá»“n Kho (Váº­t lÃ½ & Tá»•ng)
    DECLARE @idDaiLy VARCHAR(20), @idKhachHang VARCHAR(20), @ngayXuat DATETIME, @tongTien DECIMAL(18,2);
    SELECT @idDaiLy = idDaiLyBan, @idKhachHang = idKhachHang, @ngayXuat = ngayXuat FROM PhieuXuat WHERE maPhieu = @maPhieu;

    UPDATE t
    SET t.soLuongTon = t.soLuongTon - ctp.soLuong
    FROM ChiTietTon t
    JOIN ChiTietPhieuXuat ctp ON t.maHang = ctp.maHang
    WHERE ctp.maPhieu = @maPhieu AND t.maKho IN (@idDaiLy, 'KHO_TONG_AO');

    -- 3. Ghi Ná»£ Sá»• RiÃªng
    SELECT @tongTien = SUM(soLuong * giaBan) FROM ChiTietPhieuXuat WHERE maPhieu = @maPhieu;
    UPDATE PhieuXuat SET tongTien = @tongTien WHERE maPhieu = @maPhieu;

    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai)
    VALUES (@idKhachHang, @ngayXuat, 'MUA_HANG', @tongTien, N'PhÃ¡t sinh ná»£ tá»« phiáº¿u ' + @maPhieu);

    -- Cáº­p nháº­t dÆ° ná»£ lÅ©y káº¿ KhÃ¡ch hÃ ng
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe + @tongTien WHERE maDoiTuong = @idKhachHang;
END;
GO

-- 3. Method: PhieuTinhChietKhau.tinhToan() (Discount Engine)
-- MÃ´ táº£: TÃ­nh toÃ¡n chiáº¿t kháº¥u theo báº­c hoáº·c cá»‘ Ä‘á»‹nh, táº¡o báº£ng kÃª Pending [Source 7-10, 17-18, 27-28]
IF OBJECT_ID('sp_Engine_TinhChietKhau', 'P') IS NOT NULL DROP PROCEDURE sp_Engine_TinhChietKhau;
GO
CREATE PROCEDURE sp_Engine_TinhChietKhau
    @maPhieuTinh VARCHAR(20)
AS
BEGIN
    DECLARE @loaiDoiTuong VARCHAR(10), @tuNgay DATE, @denNgay DATE;
    SELECT @loaiDoiTuong = loaiDoiTuong, @tuNgay = tuNgay, @denNgay = denNgay 
    FROM PhieuTinhChietKhau WHERE maPhieuTinh = @maPhieuTinh;

    -- Logic demo cho trÆ°á»ng há»£p Quy luáº­t Cá»‘ Ä‘á»‹nh (Fixed Rate)
    IF @loaiDoiTuong = 'NCC'
    BEGIN
        INSERT INTO BangKeChietKhau (maBangKe, maPhieuTinh, maDoiTuong, tongTien, trangThai)
        SELECT 
            'BK_' + @maPhieuTinh + '_' + pn.idNhaCungCap,
            @maPhieuTinh,
            pn.idNhaCungCap,
            -- Giáº£ Ä‘á»‹nh logic: Tá»•ng SL * 1000Ä‘ (Cáº§n join báº£ng CauHinhChietKhau Ä‘á»ƒ láº¥y tham sá»‘ thá»±c)
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
            SUM(ct.soLuong * 500), -- Giáº£ Ä‘á»‹nh logic cá»‘ Ä‘á»‹nh
            'Pending'
        FROM PhieuXuat px
        JOIN ChiTietPhieuXuat ct ON px.maPhieu = ct.maPhieu
        WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay
        GROUP BY px.idKhachHang;
    END
END;
GO

-- 4. Method: QuanLyTaiChinh.tinhLoiNhuanThuc() (P&L Report)
-- MÃ´ táº£: TÃ­nh LN rÃ²ng theo cÃ´ng thá»©c: DT - GV + CK_NCC - CK_Khach - CPVH [Source 12, 20, 30]
IF OBJECT_ID('sp_TaiChinh_BaoCaoLoiNhuan', 'P') IS NOT NULL DROP PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan;
GO
CREATE PROCEDURE sp_TaiChinh_BaoCaoLoiNhuan
    @tuNgay DATE, @denNgay DATE
AS
BEGIN
    -- Doanh thu
    DECLARE @DoanhThu DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM PhieuXuat WHERE ngayXuat BETWEEN @tuNgay AND @denNgay);
    -- GiÃ¡ vá»‘n (láº¥y snapshot)
    DECLARE @GiaVon DECIMAL(18,2) = (SELECT ISNULL(SUM(soLuong * giaVonTaiThoiDiem),0) 
                                     FROM ChiTietPhieuXuat ctp JOIN PhieuXuat px ON ctp.maPhieu = px.maPhieu 
                                     WHERE px.ngayXuat BETWEEN @tuNgay AND @denNgay);
    -- CK NCC (Thu nháº­p)
    DECLARE @CK_NCC DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM BangKeChietKhau bk 
                                     JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                     WHERE pt.loaiDoiTuong = 'NCC' AND bk.trangThai = 'Approved');
    -- CK KhÃ¡ch (Chi phÃ­)
    DECLARE @CK_Khach DECIMAL(18,2) = (SELECT ISNULL(SUM(tongTien),0) FROM BangKeChietKhau bk 
                                       JOIN PhieuTinhChietKhau pt ON bk.maPhieuTinh = pt.maPhieuTinh
                                       WHERE pt.loaiDoiTuong = 'KHACH' AND bk.trangThai = 'Approved');
    -- Chi phÃ­ váº­n hÃ nh
    DECLARE @ChiPhiVH DECIMAL(18,2) = (SELECT ISNULL(SUM(soTien),0) FROM PhieuThuChi WHERE loaiPhieu = 'CHI' AND lyDo LIKE N'%Váº­n hÃ nh%');

    SELECT @DoanhThu AS DoanhThu, @GiaVon AS GiaVon, @CK_NCC AS ThuTuCK, @CK_Khach AS ChiPhiCK, @ChiPhiVH AS ChiPhiVH,
           (@DoanhThu - @GiaVon + @CK_NCC - @CK_Khach - @ChiPhiVH) AS LoiNhuanRong;
END;
GO

-- ====================================================================================
-- PHáº¦N B: CÃC STORED PROCEDURE CRUD (ThÃªm, Sá»­a, XÃ³a, Láº¥y dá»¯ liá»‡u)
-- ====================================================================================

-- 1. CRUD HANGHOA
IF OBJECT_ID('sp_HangHoa_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_Insert;
GO
CREATE PROCEDURE sp_HangHoa_Insert @maHang VARCHAR(20), @tenHang NVARCHAR(100), @donViTinh NVARCHAR(20), @quyCach NVARCHAR(50), @isDisabled BIT = 0 AS
INSERT INTO HangHoa(maHang, tenHang, donViTinh, quyCach, isDisabled) VALUES (@maHang, @tenHang, @donViTinh, @quyCach, @isDisabled);
GO

IF OBJECT_ID('sp_HangHoa_Update', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_Update;
GO
CREATE PROCEDURE sp_HangHoa_Update @maHang VARCHAR(20), @tenHang NVARCHAR(100), @donViTinh NVARCHAR(20), @quyCach NVARCHAR(50), @isDisabled BIT = 0 AS
UPDATE HangHoa SET tenHang=@tenHang, donViTinh=@donViTinh, quyCach=@quyCach, isDisabled=@isDisabled WHERE maHang=@maHang;
GO

IF OBJECT_ID('sp_HangHoa_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_Delete;
GO
CREATE PROCEDURE sp_HangHoa_Delete @maHang VARCHAR(20) AS DELETE FROM HangHoa WHERE maHang=@maHang;
GO

IF OBJECT_ID('sp_HangHoa_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_SelectAll;
GO
CREATE PROCEDURE sp_HangHoa_SelectAll AS SELECT * FROM HangHoa WHERE isDisabled = 0;
GO

IF OBJECT_ID('sp_HangHoa_SelectById', 'P') IS NOT NULL DROP PROCEDURE sp_HangHoa_SelectById;
GO
CREATE PROCEDURE sp_HangHoa_SelectById @maHang VARCHAR(20) AS SELECT * FROM HangHoa WHERE maHang=@maHang;
GO

-- 2. CRUD NHACUNGCAP
IF OBJECT_ID('sp_NhaCungCap_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_Insert;
GO
CREATE PROCEDURE sp_NhaCungCap_Insert @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @maSoThue VARCHAR(50), @soNgayDuocNo INT, @isDisabled BIT = 0 AS
INSERT INTO NhaCungCap(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo, isDisabled) VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @maSoThue, @soNgayDuocNo, @isDisabled);
GO

IF OBJECT_ID('sp_NhaCungCap_Update', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_Update;
GO
CREATE PROCEDURE sp_NhaCungCap_Update @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @maSoThue VARCHAR(50), @soNgayDuocNo INT, @isDisabled BIT = 0 AS
UPDATE NhaCungCap SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, maSoThue=@maSoThue, soNgayDuocNo=@soNgayDuocNo, isDisabled=@isDisabled WHERE maDoiTuong=@maDoiTuong;
GO

IF OBJECT_ID('sp_NhaCungCap_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_Delete;
GO
CREATE PROCEDURE sp_NhaCungCap_Delete @maDoiTuong VARCHAR(20) AS DELETE FROM NhaCungCap WHERE maDoiTuong=@maDoiTuong;
GO

IF OBJECT_ID('sp_NhaCungCap_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_NhaCungCap_SelectAll;
GO
CREATE PROCEDURE sp_NhaCungCap_SelectAll AS SELECT * FROM NhaCungCap WHERE isDisabled = 0;
GO

-- 3. CRUD KHACHHANG
IF OBJECT_ID('sp_KhachHang_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_KhachHang_Insert;
GO
CREATE PROCEDURE sp_KhachHang_Insert @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @aoNuoi NVARCHAR(100), @isDisabled BIT = 0 AS
INSERT INTO KhachHang(maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi, isDisabled) VALUES (@maDoiTuong, @tenDoiTuong, @soDienThoai, @diaChi, @aoNuoi, @isDisabled);
GO

IF OBJECT_ID('sp_KhachHang_Update', 'P') IS NOT NULL DROP PROCEDURE sp_KhachHang_Update;
GO
CREATE PROCEDURE sp_KhachHang_Update @maDoiTuong VARCHAR(20), @tenDoiTuong NVARCHAR(100), @soDienThoai VARCHAR(20), @diaChi NVARCHAR(200), @aoNuoi NVARCHAR(100), @isDisabled BIT = 0 AS
UPDATE KhachHang SET tenDoiTuong=@tenDoiTuong, soDienThoai=@soDienThoai, diaChi=@diaChi, aoNuoi=@aoNuoi, isDisabled=@isDisabled WHERE maDoiTuong=@maDoiTuong;
GO

IF OBJECT_ID('sp_KhachHang_SelectAll', 'P') IS NOT NULL DROP PROCEDURE sp_KhachHang_SelectAll;
GO
CREATE PROCEDURE sp_KhachHang_SelectAll AS SELECT * FROM KhachHang WHERE isDisabled = 0;
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
CREATE PROCEDURE sp_PhieuNhap_Insert @maPhieu VARCHAR(20), @ngayNhap DATETIME, @idDaiLyNhap VARCHAR(20), @idNhaCungCap VARCHAR(20), @trangthaithanhtoan NVARCHAR(50) = N'Chưa Thanh Toán' AS
INSERT INTO PhieuNhap(maPhieu, ngayNhap, idDaiLyNhap, idNhaCungCap, tongTien, trangthaithanhtoan) VALUES (@maPhieu, @ngayNhap, @idDaiLyNhap, @idNhaCungCap, 0, @trangthaithanhtoan);
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
CREATE PROCEDURE sp_PhieuXuat_Insert @maPhieu VARCHAR(20), @ngayXuat DATETIME, @idDaiLyBan VARCHAR(20), @idKhachHang VARCHAR(20), @trangthaithanhtoan NVARCHAR(50) = N'Chưa Thanh Toán' AS
INSERT INTO PhieuXuat(maPhieu, ngayXuat, idDaiLyBan, idKhachHang, tongTien, trangthaithanhtoan) VALUES (@maPhieu, @ngayXuat, @idDaiLyBan, @idKhachHang, 0, @trangthaithanhtoan);
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

-- =============================================
-- 7. SEED DATA (DỮ LIỆU MẪU)
-- =============================================

-- 1. Master Data: Hàng Hóa
IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE maHang = 'HH001')
INSERT INTO HangHoa (maHang, tenHang, donViTinh, quyCach, giaVonHienTai, giaBanHienTai) VALUES ('HH001', N'Tôm Sú Oxi', 'Kg', N'Thùng xốp', 180000, 220000);

IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE maHang = 'HH002')
INSERT INTO HangHoa (maHang, tenHang, donViTinh, quyCach, giaVonHienTai, giaBanHienTai) VALUES ('HH002', N'Tôm Thẻ Chân Trắng', 'Kg', N'Thùng xốp', 120000, 150000);

IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE maHang = 'HH003')
INSERT INTO HangHoa (maHang, tenHang, donViTinh, quyCach, giaVonHienTai, giaBanHienTai) VALUES ('HH003', N'Cua Cà Mau (Y1)', 'Kg', N'Sọt', 350000, 450000);

IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE maHang = 'HH004')
INSERT INTO HangHoa (maHang, tenHang, donViTinh, quyCach, giaVonHienTai, giaBanHienTai) VALUES ('HH004', N'Mực Ống A', 'Kg', N'Khay', 250000, 320000);

IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE maHang = 'HH005')
INSERT INTO HangHoa (maHang, tenHang, donViTinh, quyCach, giaVonHienTai, giaBanHienTai) VALUES ('HH005', N'Cá Hồi Nauy File', 'Kg', N'Hút chân không', 450000, 580000);


-- 2. Master Data: Đơn Vị Tính
IF NOT EXISTS (SELECT 1 FROM DonViTinh WHERE maHang = 'HH001' AND tenDonVi = N'Thùng (20kg)')
INSERT INTO DonViTinh (maHang, tenDonVi, tyLeQuyDoi, giaBan, maHangDonVi) VALUES ('HH001', N'Thùng (20kg)', 20, 4400000, 'HH001-THUNG');

IF NOT EXISTS (SELECT 1 FROM DonViTinh WHERE maHang = 'HH002' AND tenDonVi = N'Tạ (100kg)')
INSERT INTO DonViTinh (maHang, tenDonVi, tyLeQuyDoi, giaBan, maHangDonVi) VALUES ('HH002', N'Tạ (100kg)', 100, 14500000, 'HH002-TA');


-- 3. Master Data: Nhà Cung Cấp
IF NOT EXISTS (SELECT 1 FROM NhaCungCap WHERE maDoiTuong = 'NCC001')
INSERT INTO NhaCungCap (maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo, duNoLuyKe) VALUES ('NCC001', N'Trại Tôm Minh Phú', '0901234567', N'Cà Mau', '3500123456', 30, 0);

IF NOT EXISTS (SELECT 1 FROM NhaCungCap WHERE maDoiTuong = 'NCC002')
INSERT INTO NhaCungCap (maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo, duNoLuyKe) VALUES ('NCC002', N'Vựa Hải Sản Biển Đông', '0909888777', N'Vũng Tàu', '3600987654', 15, 0);

IF NOT EXISTS (SELECT 1 FROM NhaCungCap WHERE maDoiTuong = 'NCC003')
INSERT INTO NhaCungCap (maDoiTuong, tenDoiTuong, soDienThoai, diaChi, maSoThue, soNgayDuocNo, duNoLuyKe) VALUES ('NCC003', N'Công Ty XNK Thủy Sản An Giang', '0912333444', N'An Giang', '3700112233', 45, 0);


-- 4. Master Data: Khách Hàng
IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE maDoiTuong = 'KH001')
INSERT INTO KhachHang (maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi, duNoLuyKe) VALUES ('KH001', N'Nhà Hàng Biển Nhớ', '0933111222', N'Q1, TP.HCM', N'Không', 0);

IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE maDoiTuong = 'KH002')
INSERT INTO KhachHang (maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi, duNoLuyKe) VALUES ('KH002', N'Quán Nhậu Làng Chài', '0933444555', N'Q3, TP.HCM', N'Không', 0);

IF NOT EXISTS (SELECT 1 FROM KhachHang WHERE maDoiTuong = 'KH003')
INSERT INTO KhachHang (maDoiTuong, tenDoiTuong, soDienThoai, diaChi, aoNuoi, duNoLuyKe) VALUES ('KH003', N'Anh Ba (Đại Lý Cấp 1)', '0933666777', N'Bình Dương', N'Ao số 3', 0);


-- 5. Master Data: Đại Lý & Kho
IF NOT EXISTS (SELECT 1 FROM DaiLy WHERE maDaiLy = 'DL001')
INSERT INTO DaiLy (maDaiLy, tenDaiLy, loaiDaiLy) VALUES ('DL001', N'Đại Lý Nhập 1', 'NHAP_A');

IF NOT EXISTS (SELECT 1 FROM DaiLy WHERE maDaiLy = 'DL002')
INSERT INTO DaiLy (maDaiLy, tenDaiLy, loaiDaiLy) VALUES ('DL002', N'Đại Lý Bán 1', 'BAN_C');

IF NOT EXISTS (SELECT 1 FROM Kho WHERE maKho = 'K001')
INSERT INTO Kho (maKho, tenKho, loaiKho, maDaiLyPhuTrach) VALUES ('K001', N'Kho Lạnh Chính', 'VAT_LY', 'DL001');

IF NOT EXISTS (SELECT 1 FROM Kho WHERE maKho = 'KHO_TONG_AO')
INSERT INTO Kho (maKho, tenKho, loaiKho, maDaiLyPhuTrach) VALUES ('KHO_TONG_AO', N'Hệ Thống Kho Ảo', 'TONG_AO', NULL);


-- 5b. Master Data: Đối Tượng Chi Phí
IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP001')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP001', N'Tiền Điện');

IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP002')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP002', N'Tiền Nước');

IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP003')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP003', N'Tiền Lương Nhân Viên');

IF NOT EXISTS (SELECT 1 FROM DoiTuongChiPhi WHERE maDoiTuong = 'DTCP004')
INSERT INTO DoiTuongChiPhi (maDoiTuong, tenDoiTuong) VALUES ('DTCP004', N'Chi Phí Vận Chuyển');


-- 6. Transaction Data: Tồn Kho Đầu Kỳ (Upsert logic using MERGE is robust, but IF NOT EXISTS is simpler for seed)
IF NOT EXISTS (SELECT 1 FROM ChiTietTon WHERE maKho='K001' AND maHang='HH001')
INSERT INTO ChiTietTon (maKho, maHang, soLuongTon, giaTriTon) VALUES ('K001', 'HH001', 500, 180000);

IF NOT EXISTS (SELECT 1 FROM ChiTietTon WHERE maKho='K001' AND maHang='HH002')
INSERT INTO ChiTietTon (maKho, maHang, soLuongTon, giaTriTon) VALUES ('K001', 'HH002', 1000, 120000);

IF NOT EXISTS (SELECT 1 FROM ChiTietTon WHERE maKho='KHO_TONG_AO' AND maHang='HH001')
INSERT INTO ChiTietTon (maKho, maHang, soLuongTon, giaTriTon) VALUES ('KHO_TONG_AO', 'HH001', 500, 180000);

IF NOT EXISTS (SELECT 1 FROM ChiTietTon WHERE maKho='KHO_TONG_AO' AND maHang='HH002')
INSERT INTO ChiTietTon (maKho, maHang, soLuongTon, giaTriTon) VALUES ('KHO_TONG_AO', 'HH002', 1000, 120000);


-- 7. Transaction Data: Phiếu Nhập
IF NOT EXISTS (SELECT 1 FROM PhieuNhap WHERE maPhieu = 'PN001')
BEGIN
    INSERT INTO PhieuNhap (maPhieu, ngayNhap, idDaiLyNhap, idNhaCungCap, hanThanhToan, tongTien) VALUES ('PN001', DATEADD(DAY, -10, GETDATE()), 'DL001', 'NCC001', DATEADD(DAY, 20, GETDATE()), 0);
    INSERT INTO ChiTietPhieuNhap (maPhieu, maHang, soLuong, donGiaNhap) VALUES ('PN001', 'HH001', 1000, 180000), ('PN001', 'HH003', 200, 350000);
    
    -- Sync Logic (Simplified for Seed)
    UPDATE PhieuNhap SET tongTien = (1000*180000 + 200*350000) WHERE maPhieu = 'PN001';
    UPDATE NhaCungCap SET duNoLuyKe = duNoLuyKe + (1000*180000 + 200*350000) WHERE maDoiTuong = 'NCC001';
END

IF NOT EXISTS (SELECT 1 FROM PhieuNhap WHERE maPhieu = 'PN002')
BEGIN
    INSERT INTO PhieuNhap (maPhieu, ngayNhap, idDaiLyNhap, idNhaCungCap, hanThanhToan, tongTien) VALUES ('PN002', DATEADD(DAY, -5, GETDATE()), 'DL001', 'NCC002', DATEADD(DAY, 10, GETDATE()), 0);
    INSERT INTO ChiTietPhieuNhap (maPhieu, maHang, soLuong, donGiaNhap) VALUES ('PN002', 'HH002', 500, 115000), ('PN002', 'HH004', 100, 250000);

    UPDATE PhieuNhap SET tongTien = (500*115000 + 100*250000) WHERE maPhieu = 'PN002';
    UPDATE NhaCungCap SET duNoLuyKe = duNoLuyKe + (500*115000 + 100*250000) WHERE maDoiTuong = 'NCC002';
END


-- 8. Transaction Data: Phiếu Xuất
IF NOT EXISTS (SELECT 1 FROM PhieuXuat WHERE maPhieu = 'PX001')
BEGIN
    INSERT INTO PhieuXuat (maPhieu, ngayXuat, idDaiLyBan, idKhachHang, tongTien) VALUES ('PX001', DATEADD(DAY, -3, GETDATE()), 'DL002', 'KH001', 0);
    INSERT INTO ChiTietPhieuXuat (maPhieu, maHang, soLuong, giaBan, giaVonTaiThoiDiem) VALUES ('PX001', 'HH001', 100, 220000, 180000), ('PX001', 'HH003', 20, 450000, 350000);

    UPDATE PhieuXuat SET tongTien = (31000000) WHERE maPhieu = 'PX001';
    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai) VALUES ('KH001', DATEADD(DAY, -3, GETDATE()), 'MUA_HANG', 31000000, N'Mua hàng PX001');
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe + 31000000 WHERE maDoiTuong = 'KH001';
END

IF NOT EXISTS (SELECT 1 FROM PhieuXuat WHERE maPhieu = 'PX002')
BEGIN
    INSERT INTO PhieuXuat (maPhieu, ngayXuat, idDaiLyBan, idKhachHang, tongTien) VALUES ('PX002', DATEADD(DAY, -1, GETDATE()), 'DL002', 'KH003', 0);
    INSERT INTO ChiTietPhieuXuat (maPhieu, maHang, soLuong, giaBan, giaVonTaiThoiDiem) VALUES ('PX002', 'HH002', 2000, 145000, 120000), ('PX002', 'HH001', 500, 210000, 180000);

    UPDATE PhieuXuat SET tongTien = (395000000) WHERE maPhieu = 'PX002';
    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai) VALUES ('KH003', DATEADD(DAY, -1, GETDATE()), 'MUA_HANG', 395000000, N'Mua hàng PX002');
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe + 395000000 WHERE maDoiTuong = 'KH003';
END


-- 9. Transaction Data: Phiếu Thu Chi
IF NOT EXISTS (SELECT 1 FROM PhieuThuChi WHERE maPhieu = 'PT001')
BEGIN
    INSERT INTO PhieuThuChi (maPhieu, loaiPhieu, ngayLap, soTien, lyDo) VALUES ('PT001', 'THU', DATEADD(DAY, -1, GETDATE()), 50000000, N'KH003 Thanh toán đợt 1');
    INSERT INTO SoRiengKhachHang (maKhachHang, ngayGiaoDich, loaiGiaoDich, soTienPhatSinh, dienGiai) VALUES ('KH003', DATEADD(DAY, -1, GETDATE()), 'THANH_TOAN', -50000000, N'Thanh toán PT001');
    UPDATE KhachHang SET duNoLuyKe = duNoLuyKe - 50000000 WHERE maDoiTuong = 'KH003';
END

IF NOT EXISTS (SELECT 1 FROM PhieuThuChi WHERE maPhieu = 'PC001')
INSERT INTO PhieuThuChi (maPhieu, loaiPhieu, ngayLap, soTien, lyDo) VALUES ('PC001', 'CHI', DATEADD(DAY, 0, GETDATE()), 2000000, N'Chi phí điện nước tháng 1');

IF NOT EXISTS (SELECT 1 FROM PhieuThuChi WHERE maPhieu = 'PC002')
INSERT INTO PhieuThuChi (maPhieu, loaiPhieu, ngayLap, soTien, lyDo) VALUES ('PC002', 'CHI', DATEADD(DAY, 0, GETDATE()), 5000000, N'Chi phí vận hành kho');

