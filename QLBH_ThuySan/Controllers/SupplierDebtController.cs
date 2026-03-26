using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using QLBH_ThuySan.Models;
using System.IO;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Input Debt (Công Nợ Đầu Vào / Nhà Cung Cấp)
    /// Tracks Imports and Payables
    /// </summary>
    [Authorize]
    public class SupplierDebtController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: SupplierDebt - List Suppliers with Filtering and Sorting
        public async Task<IActionResult> Index(
            string? searchMaNcc, 
            string? searchTenNcc, 
            string? searchSdt, 
            string? searchDiaChi, 
            bool filterDuNo = false,
            string? sortOrder = null)
        {
            var query = _context.NhaCungCaps.AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(searchMaNcc))
                query = query.Where(n => n.MaDoiTuong != null && n.MaDoiTuong.Contains(searchMaNcc));
            
            if (!string.IsNullOrEmpty(searchTenNcc))
                query = query.Where(n => n.TenDoiTuong != null && n.TenDoiTuong.Contains(searchTenNcc));

            if (!string.IsNullOrEmpty(searchSdt))
                query = query.Where(n => n.SoDienThoai != null && n.SoDienThoai.Contains(searchSdt));
                
            if (!string.IsNullOrEmpty(searchDiaChi))
                query = query.Where(n => n.DiaChi != null && n.DiaChi.Contains(searchDiaChi));
                
            if (filterDuNo)
                query = query.Where(n => n.DuNoLuyKe > 0);

            // Sort Parameters for View
            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ma_desc" : "";
            ViewData["TenSortParm"] = sortOrder == "ten_asc" ? "ten_desc" : "ten_asc";
            ViewData["SdtSortParm"] = sortOrder == "sdt_asc" ? "sdt_desc" : "sdt_asc";
            ViewData["DuNoSortParm"] = sortOrder == "duno_asc" ? "duno_desc" : "duno_asc";

            // Sorting
            query = sortOrder switch
            {
                "ma_desc" => query.OrderByDescending(n => n.MaDoiTuong),
                "ten_asc" => query.OrderBy(n => n.TenDoiTuong),
                "ten_desc" => query.OrderByDescending(n => n.TenDoiTuong),
                "sdt_asc" => query.OrderBy(n => n.SoDienThoai),
                "sdt_desc" => query.OrderByDescending(n => n.SoDienThoai),
                "duno_asc" => query.OrderBy(n => n.DuNoLuyKe),
                "duno_desc" => query.OrderByDescending(n => n.DuNoLuyKe),
                _ => query.OrderBy(n => n.MaDoiTuong)
            };

            var suppliers = await query.ToListAsync();
            
            // Sync/Default DuNoLuyKe if null
            foreach (var s in suppliers)
            {
                s.DuNoLuyKe ??= 0;
            }

            // Store filter state for view
            ViewData["searchMaNcc"] = searchMaNcc;
            ViewData["searchTenNcc"] = searchTenNcc;
            ViewData["searchSdt"] = searchSdt;
            ViewData["searchDiaChi"] = searchDiaChi;
            ViewData["filterDuNo"] = filterDuNo;

            return View(suppliers);
        }

        // GET: SupplierDebt/Details/NCC001 - View Debt Details (Imports history)
        [HttpGet("/SupplierDebt/Details/{id?}")]
        public async Task<IActionResult> Details(
            string? id,
            string? pnMaPhieu, DateTime? pnFromDate, DateTime? pnToDate, string? pnStatus, string? pnSortOrder,
            DateTime? entryFromDate, DateTime? entryToDate, string? entryLoai, string? entryDienGiai, string? entrySortOrder,
            string? dsMaPhieu, DateTime? dsFromDate, DateTime? dsToDate, string? dsStatus, string? dsSortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.NhaCungCaps
                .FirstOrDefaultAsync(m => m.MaDoiTuong == id);

            if (supplier == null)
            {
                return NotFound();
            }

            // 1. Initial Load
            var entries = await _context.SoRiengNhaCungCaps
                .Where(e => e.MaNhaCungCap == id)
                .OrderByDescending(e => e.NgayGiaoDich)
                .ToListAsync();

            var imports = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(c => c.MaHangNavigation)
                .Where(p => p.IdNhaCungCap == id && !p.IsDisabled)
                .OrderByDescending(p => p.NgayNhap)
                .ToListAsync();

            // 2. Auto-correct logic: Sync PhieuNhap to SoRiengNhaCungCap
            bool needsSave = false;
            foreach (var pn in imports)
            {
                bool hasEntry = entries.Any(e => e.LoaiGiaoDich == "MUA_HANG" && 
                                               (e.SoTienPhatSinh == pn.SoPhaiThanhToan && e.NgayGiaoDich?.Date == pn.NgayNhap?.Date) ||
                                               (e.DienGiai != null && e.DienGiai.Contains(pn.MaPhieu)));
                if (!hasEntry)
                {
                    var newEntry = new SoRiengNhaCungCap
                    {
                        MaNhaCungCap = id,
                        NgayGiaoDich = pn.NgayNhap ?? DateTime.Now,
                        LoaiGiaoDich = "MUA_HANG",
                        SoTienPhatSinh = pn.SoPhaiThanhToan,
                        DienGiai = $"Tự động ghi nợ nhập hàng {pn.MaPhieu}"
                    };
                    _context.SoRiengNhaCungCaps.Add(newEntry);
                    entries.Add(newEntry);
                    needsSave = true;

                    if (pn.TrangThaiThanhToan == "Đã Thanh Toán")
                    {
                        var paymentEntry = new SoRiengNhaCungCap
                        {
                            MaNhaCungCap = id,
                            NgayGiaoDich = pn.NgayNhap ?? DateTime.Now,
                            LoaiGiaoDich = "THANH_TOAN",
                            SoTienPhatSinh = pn.SoPhaiThanhToan,
                            DienGiai = $"Thanh toán ngay cho phiếu {pn.MaPhieu}"
                        };
                        _context.SoRiengNhaCungCaps.Add(paymentEntry);
                        entries.Add(paymentEntry);
                    }
                }
            }

            // Sync DuNoLuyKe
            if (needsSave || (supplier.DuNoLuyKe ?? 0) <= 0 || entries.Count > 0)
            {
                decimal calculatedDebt = 0;
                foreach (var entry in entries.OrderBy(e => e.NgayGiaoDich))
                {
                    if (entry.LoaiGiaoDich == "MUA_HANG")
                        calculatedDebt += entry.SoTienPhatSinh ?? 0;
                    else if (entry.LoaiGiaoDich == "THANH_TOAN" || entry.LoaiGiaoDich == "CAN_TRU" || entry.LoaiGiaoDich == "THU_CK")
                        calculatedDebt -= entry.SoTienPhatSinh ?? 0;
                }
                
                if (supplier.DuNoLuyKe != calculatedDebt)
                {
                    supplier.DuNoLuyKe = calculatedDebt;
                    _context.Update(supplier);
                    needsSave = true;
                }
            }

            if (needsSave)
            {
                await _context.SaveChangesAsync();
                // Refresh lists
                entries = await _context.SoRiengNhaCungCaps.Where(e => e.MaNhaCungCap == id).ToListAsync();
                imports = await _context.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps).ThenInclude(c => c.MaHangNavigation)
                    .Where(p => p.IdNhaCungCap == id && !p.IsDisabled).ToListAsync();
            }

            // 3. Apply Filters and Sorting to Import History (imports)
            if (!string.IsNullOrEmpty(pnMaPhieu)) imports = [.. imports.Where(p => p.MaPhieu != null && p.MaPhieu.Contains(pnMaPhieu))];
            if (pnFromDate.HasValue) imports = [.. imports.Where(p => p.NgayNhap?.Date >= pnFromDate.Value.Date)];
            if (pnToDate.HasValue) imports = [.. imports.Where(p => p.NgayNhap?.Date <= pnToDate.Value.Date)];
            if (!string.IsNullOrEmpty(pnStatus)) imports = [.. imports.Where(p => p.TrangThaiThanhToan == pnStatus)];

            ViewData["PNSort_Ma"] = pnSortOrder == "ma_asc" ? "ma_desc" : "ma_asc";
            ViewData["PNSort_Date"] = string.IsNullOrEmpty(pnSortOrder) || pnSortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["PNSort_Status"] = pnSortOrder == "status_asc" ? "status_desc" : "status_asc";
            ViewData["PNSort_Total"] = pnSortOrder == "total_asc" ? "total_desc" : "total_asc";
            ViewData["PNSort_Paid"] = pnSortOrder == "paid_asc" ? "paid_desc" : "paid_asc";
            ViewData["PNSort_Debt"] = pnSortOrder == "debt_asc" ? "debt_desc" : "debt_asc";

            imports = pnSortOrder switch {
                "ma_asc" => [.. imports.OrderBy(p => p.MaPhieu)],
                "ma_desc" => [.. imports.OrderByDescending(p => p.MaPhieu)],
                "date_asc" => [.. imports.OrderBy(p => p.NgayNhap)],
                "date_desc" => [.. imports.OrderByDescending(p => p.NgayNhap)],
                "status_asc" => [.. imports.OrderBy(p => p.TrangThaiThanhToan)],
                "status_desc" => [.. imports.OrderByDescending(p => p.TrangThaiThanhToan)],
                "total_asc" => [.. imports.OrderBy(p => p.SoPhaiThanhToan)],
                "total_desc" => [.. imports.OrderByDescending(p => p.SoPhaiThanhToan)],
                "paid_asc" => [.. imports.OrderBy(p => p.SoDaThanhToan)],
                "paid_desc" => [.. imports.OrderByDescending(p => p.SoDaThanhToan)],
                "debt_asc" => [.. imports.OrderBy(p => p.SoChuaThanhToan)],
                "debt_desc" => [.. imports.OrderByDescending(p => p.SoChuaThanhToan)],
                _ => [.. imports.OrderByDescending(p => p.NgayNhap)]
            };

            // 4. Apply Filters and Sorting to Transaction History (entries)
            if (entryFromDate.HasValue) entries = [.. entries.Where(e => e.NgayGiaoDich?.Date >= entryFromDate.Value.Date)];
            if (entryToDate.HasValue) entries = [.. entries.Where(e => e.NgayGiaoDich?.Date <= entryToDate.Value.Date)];
            if (!string.IsNullOrEmpty(entryLoai)) entries = [.. entries.Where(e => e.LoaiGiaoDich == entryLoai)];
            if (!string.IsNullOrEmpty(entryDienGiai)) entries = [.. entries.Where(e => e.DienGiai != null && e.DienGiai.Contains(entryDienGiai))];

            ViewData["EntrySort_Id"] = entrySortOrder == "id_asc" ? "id_desc" : "id_asc";
            ViewData["EntrySort_Date"] = string.IsNullOrEmpty(entrySortOrder) || entrySortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["EntrySort_Type"] = entrySortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewData["EntrySort_Amount"] = entrySortOrder == "amount_asc" ? "amount_desc" : "amount_asc";
            ViewData["EntrySort_Desc"] = entrySortOrder == "desc_asc" ? "desc_desc" : "desc_asc";

            entries = entrySortOrder switch {
                "id_asc" => [.. entries.OrderBy(e => e.Id)],
                "id_desc" => [.. entries.OrderByDescending(e => e.Id)],
                "date_asc" => [.. entries.OrderBy(e => e.NgayGiaoDich)],
                "date_desc" => [.. entries.OrderByDescending(e => e.NgayGiaoDich)],
                "type_asc" => [.. entries.OrderBy(e => e.LoaiGiaoDich)],
                "type_desc" => [.. entries.OrderByDescending(e => e.LoaiGiaoDich)],
                "amount_asc" => [.. entries.OrderBy(e => e.SoTienPhatSinh)],
                "amount_desc" => [.. entries.OrderByDescending(e => e.SoTienPhatSinh)],
                "desc_asc" => [.. entries.OrderBy(e => e.DienGiai)],
                "desc_desc" => [.. entries.OrderByDescending(e => e.DienGiai)],
                _ => [.. entries.OrderByDescending(e => e.NgayGiaoDich)]
            };

            // 5. Apply Filters and Sorting to Discounts (PhieuTinhChietKhau)
            var discounts = await _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietChietKhaus)
                    .ThenInclude(c => c.MaHangNavigation)
                .Where(p => p.MaDoiTuong == id && p.LoaiDoiTuong == "NCC")
                .ToListAsync();

            if (!string.IsNullOrEmpty(dsMaPhieu)) discounts = [.. discounts.Where(d => d.MaPhieuTinh != null && d.MaPhieuTinh.Contains(dsMaPhieu))];
            if (dsFromDate.HasValue) discounts = [.. discounts.Where(d => d.NgayTao?.Date >= dsFromDate.Value.Date)];
            if (dsToDate.HasValue) discounts = [.. discounts.Where(d => d.NgayTao?.Date <= dsToDate.Value.Date)];
            if (!string.IsNullOrEmpty(dsStatus)) discounts = [.. discounts.Where(d => d.TrangThai == dsStatus)];

            ViewData["DSSort_Ma"] = dsSortOrder == "ma_asc" ? "ma_desc" : "ma_asc";
            ViewData["DSSort_Date"] = string.IsNullOrEmpty(dsSortOrder) || dsSortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["DSSort_Status"] = dsSortOrder == "status_asc" ? "status_desc" : "status_asc";
            ViewData["DSSort_Total"] = dsSortOrder == "total_asc" ? "total_desc" : "total_asc";
            ViewData["DSSort_Paid"] = dsSortOrder == "paid_asc" ? "paid_desc" : "paid_asc";
            ViewData["DSSort_Debt"] = dsSortOrder == "debt_asc" ? "debt_desc" : "debt_asc";

            discounts = dsSortOrder switch {
                "ma_asc" => [.. discounts.OrderBy(d => d.MaPhieuTinh)],
                "ma_desc" => [.. discounts.OrderByDescending(d => d.MaPhieuTinh)],
                "date_asc" => [.. discounts.OrderBy(d => d.NgayTao)],
                "date_desc" => [.. discounts.OrderByDescending(d => d.NgayTao)],
                "status_asc" => [.. discounts.OrderBy(d => d.TrangThai)],
                "status_desc" => [.. discounts.OrderByDescending(d => d.TrangThai)],
                "total_asc" => [.. discounts.OrderBy(d => d.SoPhaiThanhToan)],
                "total_desc" => [.. discounts.OrderByDescending(d => d.SoPhaiThanhToan)],
                "paid_asc" => [.. discounts.OrderBy(d => d.SoDaThanhToan)],
                "paid_desc" => [.. discounts.OrderByDescending(d => d.SoDaThanhToan)],
                "debt_asc" => [.. discounts.OrderBy(d => d.SoChuaThanhToan)],
                "debt_desc" => [.. discounts.OrderByDescending(d => d.SoChuaThanhToan)],
                _ => [.. discounts.OrderByDescending(d => d.NgayTao)]
            };

            foreach (var d in discounts)
            {
                if (d.SoChuaThanhToan == null || d.SoChuaThanhToan == 0)
                {
                    var debt = (d.SoPhaiThanhToan ?? 0) - (d.SoDaThanhToan ?? 0);
                    if (debt > 0) d.SoChuaThanhToan = debt;
                }
            }

            ViewData["pnMaPhieu"] = pnMaPhieu;
            ViewData["pnFromDate"] = pnFromDate?.ToString("yyyy-MM-dd");
            ViewData["pnToDate"] = pnToDate?.ToString("yyyy-MM-dd");
            ViewData["pnStatus"] = pnStatus;
            ViewData["pnSortOrder"] = pnSortOrder;

            ViewData["entryFromDate"] = entryFromDate?.ToString("yyyy-MM-dd");
            ViewData["entryToDate"] = entryToDate?.ToString("yyyy-MM-dd");
            ViewData["entryLoai"] = entryLoai;
            ViewData["entryDienGiai"] = entryDienGiai;
            ViewData["entrySortOrder"] = entrySortOrder;

            ViewData["dsMaPhieu"] = dsMaPhieu;
            ViewData["dsFromDate"] = dsFromDate?.ToString("yyyy-MM-dd");
            ViewData["dsToDate"] = dsToDate?.ToString("yyyy-MM-dd");
            ViewData["dsStatus"] = dsStatus;
            ViewData["dsSortOrder"] = dsSortOrder;

            ViewBag.Entries = entries;
            ViewBag.Imports = imports;
            ViewBag.Discounts = discounts;
            return View(supplier);
        }

        // POST: SupplierDebt/AddEntry
        [HttpPost("/SupplierDebt/AddEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(string maNhaCungCap, string loaiGiaoDich, decimal soTienPhatSinh, string? dienGiai)
        {
            soTienPhatSinh = Math.Abs(soTienPhatSinh);
            var ledgerEntry = new SoRiengNhaCungCap
            {
                MaNhaCungCap = maNhaCungCap,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = loaiGiaoDich,
                SoTienPhatSinh = soTienPhatSinh,
                DienGiai = dienGiai
            };
            _context.Add(ledgerEntry);

            var supplier = await _context.NhaCungCaps.FindAsync(maNhaCungCap);
            if (supplier != null)
            {
                // MUA_HANG increases debt (+); THANH_TOAN/CAN_TRU decreases debt (-)
                if (loaiGiaoDich == "MUA_HANG")
                {
                    supplier.DuNoLuyKe = (supplier.DuNoLuyKe ?? 0) + soTienPhatSinh;
                }
                else if (loaiGiaoDich == "THANH_TOAN" || loaiGiaoDich == "CAN_TRU" || loaiGiaoDich == "THU_CK")
                {
                    supplier.DuNoLuyKe = (supplier.DuNoLuyKe ?? 0) - soTienPhatSinh;
                    // Create PhieuThuChi (CHI) if it's a payment
                    if (loaiGiaoDich == "THANH_TOAN")
                    {
                        var paymentVoucher = new PhieuThuChi
                        {
                            MaPhieu = "PC_" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                            LoaiPhieu = "CHI PHIEU NHAP",
                            NgayLap = DateTime.Now,
                            SoTien = soTienPhatSinh,
                            LyDo = string.IsNullOrEmpty(dienGiai) ? "Chi tiền thanh toán cho NCC " + maNhaCungCap : dienGiai,
                            MaDoiTuong = maNhaCungCap,
                            LoaiDoiTuong = "NCC"
                        };
                        _context.PhieuThuChis.Add(paymentVoucher);
                    }

                    // FIFO distribution to unpaid PhieuNhaps
                    var unpaidInvoices = await _context.PhieuNhaps
                        .Where(p => p.IdNhaCungCap == maNhaCungCap && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán" && !p.IsDisabled)
                        .OrderBy(p => p.NgayNhap)
                        .ToListAsync();
                    
                    decimal remainingPayment = soTienPhatSinh;
                    foreach(var inv in unpaidInvoices)
                    {
                        if (remainingPayment <= 0) break;
                        var debt = inv.SoChuaThanhToan ?? 0;
                        if (remainingPayment >= debt)
                        {
                            remainingPayment -= debt;
                            inv.SoDaThanhToan = (inv.SoDaThanhToan ?? 0) + debt;
                            inv.SoChuaThanhToan = 0;
                            inv.TrangThaiThanhToan = "Đã Thanh Toán";
                            inv.NgayThanhToan = DateTime.Now;
                        }
                        else
                        {
                            inv.SoDaThanhToan = (inv.SoDaThanhToan ?? 0) + remainingPayment;
                            inv.SoChuaThanhToan -= remainingPayment;
                            remainingPayment = 0;
                            if (inv.SoChuaThanhToan > 0) inv.TrangThaiThanhToan = "Thanh Toán Một Phần";
                        }
                        _context.Update(inv);
                    }
                }
                _context.Update(supplier);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = maNhaCungCap });
        }

        // POST: SupplierDebt/PayDiscount - Receive discount money from NCC (THU)
        [HttpPost("/SupplierDebt/PayDiscount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDiscount(string maNhaCungCap, string maPhieuTinh, decimal amount, string? dienGiai)
        {
            if (amount <= 0) return RedirectToAction(nameof(Details), new { id = maNhaCungCap });

            var phieu = await _context.PhieuTinhChietKhaus.FindAsync(maPhieuTinh);
            if (phieu == null) return NotFound();

            // 1. Create PhieuThuChi (THU) - We receive money from NCC
            var paymentVoucher = new PhieuThuChi
            {
                MaPhieu = "PT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                LoaiPhieu = "THU CHIET KHAU NCC",
                NgayLap = DateTime.Now,
                SoTien = amount,
                LyDo = string.IsNullOrEmpty(dienGiai) ? $"Thu tiền chiết khấu từ NCC cho phiếu {maPhieuTinh}" : dienGiai,
                MaDoiTuong = maNhaCungCap,
                LoaiDoiTuong = "NCC"
            };
            _context.PhieuThuChis.Add(paymentVoucher);

            // 2. Update Discount Voucher
            phieu.SoDaThanhToan = (phieu.SoDaThanhToan ?? 0) + amount;
            phieu.SoChuaThanhToan = (phieu.SoPhaiThanhToan ?? 0) - phieu.SoDaThanhToan;
            phieu.NgayThanhToan = DateTime.Now;

            if (phieu.SoChuaThanhToan <= 0)
            {
                phieu.TrangThai = "Đã Thanh Toán";
                phieu.SoChuaThanhToan = 0;
            }
            else
            {
                phieu.TrangThai = "Thanh Toán Một Phần";
            }
            _context.Update(phieu);

            // 3. Create Ledger Entry (THU_CK)
            var ledgerEntry = new SoRiengNhaCungCap
            {
                MaNhaCungCap = maNhaCungCap,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = "THU_CK",
                SoTienPhatSinh = amount,
                DienGiai = string.IsNullOrEmpty(dienGiai) ? $"Thu tiền chiết khấu từ phiếu {maPhieuTinh}" : dienGiai
            };
            _context.SoRiengNhaCungCaps.Add(ledgerEntry);

            // 4. Update NCC DuNoLuyKe (Receipt from NCC reduces what we owe)
            var ncc = await _context.NhaCungCaps.FindAsync(maNhaCungCap);
            if (ncc != null)
            {
                ncc.DuNoLuyKe = (ncc.DuNoLuyKe ?? 0) - amount;
                _context.Update(ncc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = maNhaCungCap });
        }

        // POST: SupplierDebt/PayBill - Pay for a PhieuNhap (CHI)
        [HttpPost("/SupplierDebt/PayBill")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBill(string id, decimal amount, string? dienGiai)
        {
            var pn = await _context.PhieuNhaps.FirstOrDefaultAsync(p => p.MaPhieu == id);
            if (pn == null) return NotFound();

            if (amount <= 0) amount = pn.SoChuaThanhToan ?? 0;
            if (amount <= 0) return RedirectToAction(nameof(Details), new { id = pn.IdNhaCungCap });

            // 1. Update PhieuNhap
            pn.SoDaThanhToan = (pn.SoDaThanhToan ?? 0) + amount;
            pn.SoChuaThanhToan = (pn.SoPhaiThanhToan ?? 0) - pn.SoDaThanhToan;
            pn.NgayThanhToan = DateTime.Now;

            if (pn.SoChuaThanhToan <= 0)
            {
                pn.TrangThaiThanhToan = "Đã Thanh Toán";
                pn.SoChuaThanhToan = 0;
            }
            else
            {
                pn.TrangThaiThanhToan = "Thanh Toán Một Phần";
            }
            _context.Update(pn);

            // 2. Create Ledger Entry (THANH_TOAN)
            var ledgerEntry = new SoRiengNhaCungCap
            {
                MaNhaCungCap = pn.IdNhaCungCap,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = "THANH_TOAN",
                SoTienPhatSinh = amount,
                DienGiai = string.IsNullOrEmpty(dienGiai) ? $"Thanh toán cho phiếu nhập {pn.MaPhieu}" : dienGiai
            };
            _context.SoRiengNhaCungCaps.Add(ledgerEntry);

            // 3. Create PhieuThuChi (CHI)
            var paymentVoucher = new PhieuThuChi
            {
                MaPhieu = "PC_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                LoaiPhieu = "CHI PHIEU NHAP",
                NgayLap = DateTime.Now,
                SoTien = amount,
                LyDo = ledgerEntry.DienGiai,
                MaDoiTuong = pn.IdNhaCungCap,
                LoaiDoiTuong = "NCC"
            };
            _context.PhieuThuChis.Add(paymentVoucher);

            // 4. Update NCC DuNoLuyKe
            var ncc = await _context.NhaCungCaps.FindAsync(pn.IdNhaCungCap);
            if (ncc != null)
            {
                ncc.DuNoLuyKe = (ncc.DuNoLuyKe ?? 0) - amount;
                _context.Update(ncc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = pn.IdNhaCungCap });
        }

        // GET: SupplierDebt/Print
        [HttpGet("/SupplierDebt/Print")]
        public async Task<IActionResult> Print(string id, [FromQuery] int[] selectedEntries, [FromQuery] string[] selectedImports, [FromQuery] string[] selectedDiscounts)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            
            var supplier = await _context.NhaCungCaps.FindAsync(id);
            if (supplier == null) return NotFound();

            var entries = new List<SoRiengNhaCungCap>();
            if (selectedEntries != null && selectedEntries.Length > 0)
            {
                entries = await _context.SoRiengNhaCungCaps
                    .Where(e => selectedEntries.Contains(e.Id))
                    .OrderBy(e => e.NgayGiaoDich)
                    .ToListAsync();
            }

            var imports = new List<PhieuNhap>();
            if (selectedImports != null && selectedImports.Length > 0)
            {
                imports = await _context.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps).ThenInclude(c => c.MaHangNavigation)
                    .Where(p => selectedImports.Contains(p.MaPhieu))
                    .OrderBy(p => p.NgayNhap)
                    .ToListAsync();
            }

            var phieuChiets = new List<PhieuTinhChietKhau>();
            if (selectedDiscounts != null && selectedDiscounts.Length > 0)
            {
                phieuChiets = await _context.PhieuTinhChietKhaus
                    .Include(p => p.ChiTietChietKhaus)
                        .ThenInclude(c => c.MaHangNavigation)
                    .Where(p => selectedDiscounts.Contains(p.MaPhieuTinh))
                    .OrderBy(p => p.NgayTao)
                    .ToListAsync();
            }

            // 1. Auto-fetch imports mentioned in manual entries
            foreach (var entry in entries.Where(e => e.LoaiGiaoDich == "MUA_HANG"))
            {
                var words = entry.DienGiai?.Split(' ');
                if (words != null)
                {
                    foreach (var word in words)
                    {
                        if (word.StartsWith("PN") && !imports.Any(p => p.MaPhieu == word))
                        {
                            var pn = await _context.PhieuNhaps
                                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(c => c.MaHangNavigation)
                                .FirstOrDefaultAsync(p => p.MaPhieu == word);
                            if (pn != null) imports.Add(pn);
                        }
                    }
                }
            }

            // 2. Auto-fetch and include referenced discount slips
            foreach (var entry in entries.Where(e => e.LoaiGiaoDich == "THU_CK"))
            {
                var words = entry.DienGiai?.Split(' ');
                if (words != null)
                {
                    foreach (var word in words)
                    {
                        if (word.StartsWith("CK") && !phieuChiets.Any(p => p.MaPhieuTinh == word))
                        {
                            var ck = await _context.PhieuTinhChietKhaus
                                .Include(p => p.ChiTietChietKhaus).ThenInclude(c => c.MaHangNavigation)
                                .FirstOrDefaultAsync(p => p.MaPhieuTinh == word);
                            if (ck != null) phieuChiets.Add(ck);
                        }
                    }
                }
            }

            ViewBag.Entries = entries;
            ViewBag.Imports = imports.OrderBy(p => p.NgayNhap).ToList();
            ViewBag.PhieuChiets = phieuChiets.OrderBy(p => p.NgayTao).ToList();
            return View(supplier);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string id,
            DateTime? entryFromDate, DateTime? entryToDate, string? entryLoai, string? entryDienGiai)
        {
            var supplier = await _context.NhaCungCaps.FindAsync(id);
            if (supplier == null) return NotFound();

            var query = _context.SoRiengNhaCungCaps.Where(e => e.MaNhaCungCap == id);

            if (entryFromDate.HasValue) query = query.Where(e => e.NgayGiaoDich >= entryFromDate.Value.Date);
            if (entryToDate.HasValue) query = query.Where(e => e.NgayGiaoDich <= entryToDate.Value.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrEmpty(entryLoai)) query = query.Where(e => e.LoaiGiaoDich == entryLoai);
            if (!string.IsNullOrEmpty(entryDienGiai)) query = query.Where(e => e.DienGiai != null && e.DienGiai.Contains(entryDienGiai));

            var entries = await query.OrderBy(e => e.NgayGiaoDich).ToListAsync();

            var data = entries.Select(e => new {
                Ngay = e.NgayGiaoDich?.ToString("dd/MM/yyyy HH:mm"),
                Loai = e.LoaiGiaoDich == "MUA_HANG" ? "Ghi Nợ (Mua Hàng)" :
                       e.LoaiGiaoDich == "THANH_TOAN" ? "Thanh Toán" :
                       e.LoaiGiaoDich == "CAN_TRU" ? "Cấn Trừ" :
                       e.LoaiGiaoDich == "THU_CK" ? "Thu Chiết Khấu" : e.LoaiGiaoDich,
                SoTien = e.SoTienPhatSinh,
                DienGiai = e.DienGiai
            }).ToList();

            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(data);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CongNo_NCC_{id}.xlsx");
        }
    }
}
