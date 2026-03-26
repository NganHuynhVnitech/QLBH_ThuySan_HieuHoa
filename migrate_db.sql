-- 4. Re-run migration with broader patterns
-- THU Patterns
UPDATE PhieuThuChi SET loaiPhieu = 'THU BAN HANG' 
WHERE (loaiPhieu IN ('THU', 'Thu', 'CHI')) -- Handling the mislabeled CHI starting with PT_
  AND (lyDo LIKE N'%PX%' OR lyDo LIKE N'%bán hàng%' OR lyDo LIKE N'%khách%' OR maPhieu LIKE 'PT_%' AND maDoiTuong LIKE 'KH%');

UPDATE PhieuThuChi SET loaiPhieu = 'THU CHIET KHAU NCC' 
WHERE (loaiPhieu IN ('THU', 'Thu')) 
  AND (lyDo LIKE N'%chiết khấu%' OR lyDo LIKE '%ck%' OR lyDo LIKE N'%CK%');

UPDATE PhieuThuChi SET loaiPhieu = 'THU XUAT TRA NCC' 
WHERE (loaiPhieu IN ('THU', 'Thu')) 
  AND (lyDo LIKE N'%xuất trả%' OR lyDo LIKE N'%trả hàng%');

-- CHI / NHAP Patterns
UPDATE PhieuThuChi SET loaiPhieu = 'CHI CHIET KHAU KHACH HANG' 
WHERE (loaiPhieu IN ('CHI', 'Chi', 'NHAP')) 
  AND (lyDo LIKE N'%chiết khấu%' OR lyDo LIKE '%ck%' OR lyDo LIKE N'%CK%');

UPDATE PhieuThuChi SET loaiPhieu = 'CHI PHIEU NHAP' 
WHERE (loaiPhieu IN ('CHI', 'Chi', 'NHAP')) 
  AND (lyDo LIKE N'%PN%' OR lyDo LIKE N'%nhập%' OR lyDo LIKE N'%NCC%');

UPDATE PhieuThuChi SET loaiPhieu = 'CHI PHI' 
WHERE (loaiPhieu IN ('CHI', 'Chi', 'NHAP', 'CHI PHI')) 
  AND (lyDo LIKE N'%điện%' OR lyDo LIKE N'%nước%' OR lyDo LIKE N'%lương%' OR lyDo LIKE N'%xăng%' OR lyDo LIKE N'%xe%' OR lyDo LIKE N'%chi phí%');

-- Fallback for NHAP
UPDATE PhieuThuChi SET loaiPhieu = 'CHI PHI' 
WHERE loaiPhieu = 'NHAP';
GO

-- Final check
SELECT TOP 20 maPhieu, loaiPhieu, maDoiTuong, loaiDoiTuong, lyDo FROM PhieuThuChi ORDER BY ngayLap DESC;
GO
