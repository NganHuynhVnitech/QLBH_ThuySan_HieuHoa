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
    
    -- Tự động tạo Kho nếu chưa tồn tại
    IF NOT EXISTS (SELECT 1 FROM Kho WHERE maKho = @maKhoVatLy)
    BEGIN
        DECLARE @tenDaiLy NVARCHAR(200);
        SELECT @tenDaiLy = tenDaiLy FROM DaiLy WHERE maDaiLy = @idDaiLy;
        INSERT INTO Kho (maKho, tenKho, loaiKho, maDaiLyPhuTrach)
        VALUES (@maKhoVatLy, N'Kho ' + ISNULL(@tenDaiLy, @maKhoVatLy), 'VAT_LY', @idDaiLy);
    END

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

        -- Công thức Moving Weighted Average
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
