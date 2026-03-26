using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using QLBH_ThuySan.Models;
using System.IO;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Customer Debt (Công Nợ Khách Hàng)
    /// Maps to SoRiengKhachHang table in HieuHoaDB
    /// </summary>
    [Authorize]
    public class CustomerDebtController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: PrivateLedger - List all customers with their ledger entries
        [HttpGet("/PrivateLedger")]
        public async Task<IActionResult> Index(
            string? searchMaKh, 
            string? searchTenKh, 
            string? searchSdt, 
            string? searchDiaChi, 
            string? searchAoNuoi, 
            bool filterDuNo = false,
            string? sortOrder = null)
        {
            var query = _context.KhachHangs.AsQueryable();

            // Run global auto-recovery for all customers to ensure DuNoLuyKe is synced
            // In a production environment with millions of rows, this should be a background job or one-time migration.
            var allPhieuXuats = await _context.PhieuXuats.Where(p => p.LoaiXuat == "SALES").ToListAsync();
            var allEntries = await _context.SoRiengKhachHangs.ToListAsync();
            List<KhachHang> customersToUpdate = [];
            bool globalNeedsSave = false;

            foreach (var kh in await query.ToListAsync())
            {
                List<PhieuXuat> pxs = [.. allPhieuXuats.Where(p => p.IdKhachHang == kh.MaDoiTuong)];
                List<SoRiengKhachHang> entries = [.. allEntries.Where(e => e.MaKhachHang == kh.MaDoiTuong)];
                bool khNeedsSave = false;
                
                // Fix missing entries for PhieuXuat created via ExportController previously
                foreach (var px in pxs)
                {
                    bool hasEntry = entries.Any(e => e.LoaiGiaoDich == "MUA_HANG" && 
                                                   (e.SoTienPhatSinh == px.SoPhaiThanhToan && e.NgayGiaoDich?.Date == px.NgayXuat?.Date) ||
                                                   (e.DienGiai != null && e.DienGiai.Contains(px.MaPhieu)));
                    if (!hasEntry)
                    {
                        var newEntry = new SoRiengKhachHang
                        {
                            MaKhachHang = kh.MaDoiTuong,
                            NgayGiaoDich = px.NgayXuat ?? DateTime.Now,
                            LoaiGiaoDich = "MUA_HANG",
                            SoTienPhatSinh = px.SoPhaiThanhToan,
                            DienGiai = $"Tự động ghi nợ xuất bán hàng {px.MaPhieu} (Auto-recovered)"
                        };
                        _context.SoRiengKhachHangs.Add(newEntry);
                        entries.Add(newEntry);
                        khNeedsSave = true;

                        if (px.TrangThaiThanhToan == "Đã Thanh Toán")
                        {
                            var paymentEntry = new SoRiengKhachHang
                            {
                                MaKhachHang = kh.MaDoiTuong,
                                NgayGiaoDich = px.NgayXuat ?? DateTime.Now,
                                LoaiGiaoDich = "THANH_TOAN",
                                SoTienPhatSinh = px.SoPhaiThanhToan,
                                DienGiai = $"Thanh toán ngay cho phiếu {px.MaPhieu} (Auto-recovered)"
                            };
                            _context.SoRiengKhachHangs.Add(paymentEntry);
                            entries.Add(paymentEntry);
                        }
                    }
                }

                foreach (var entry in entries)
                {
                    if (entry.SoTienPhatSinh < 0)
                    {
                        entry.SoTienPhatSinh = Math.Abs(entry.SoTienPhatSinh.Value);
                        _context.Update(entry);
                        khNeedsSave = true;
                    }
                    if (entry.LoaiGiaoDich == "Mua Hàng")
                    {
                        entry.LoaiGiaoDich = "MUA_HANG";
                        _context.Update(entry);
                        khNeedsSave = true;
                    }
                }

                if (khNeedsSave || kh.DuNoLuyKe < 0 || entries.Count > 0)
                {
                    decimal calculatedDebt = 0;
                    foreach (var entry in entries.OrderBy(e => e.NgayGiaoDich))
                    {
                        if (entry.LoaiGiaoDich == "MUA_HANG" || entry.LoaiGiaoDich == "Mua Hàng")
                            calculatedDebt += entry.SoTienPhatSinh ?? 0;
                        else if (entry.LoaiGiaoDich == "THANH_TOAN" || entry.LoaiGiaoDich == "CAN_TRU" || entry.LoaiGiaoDich == "CHI_CK")
                            calculatedDebt -= entry.SoTienPhatSinh ?? 0;
                    }
                    
                    if (kh.DuNoLuyKe != calculatedDebt)
                    {
                        kh.DuNoLuyKe = calculatedDebt;
                        _context.Update(kh);
                        khNeedsSave = true;
                    }
                }

                if (khNeedsSave)
                {
                    globalNeedsSave = true;
                }
            }

            if (globalNeedsSave)
            {
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(searchMaKh))
                query = query.Where(k => k.MaDoiTuong != null && k.MaDoiTuong.Contains(searchMaKh));
            
            if (!string.IsNullOrEmpty(searchTenKh))
                query = query.Where(k => k.TenDoiTuong != null && k.TenDoiTuong.Contains(searchTenKh));

            if (!string.IsNullOrEmpty(searchSdt))
                query = query.Where(k => k.SoDienThoai != null && k.SoDienThoai.Contains(searchSdt));
                
            if (!string.IsNullOrEmpty(searchDiaChi))
                query = query.Where(k => k.DiaChi != null && k.DiaChi.Contains(searchDiaChi));
                
            if (!string.IsNullOrEmpty(searchAoNuoi))
                query = query.Where(k => k.AoNuoi != null && k.AoNuoi.Contains(searchAoNuoi));
                
            if (filterDuNo)
                query = query.Where(k => k.DuNoLuyKe > 0);

            ViewData["searchMaKh"] = searchMaKh;
            ViewData["searchTenKh"] = searchTenKh;
            ViewData["searchSdt"] = searchSdt;
            ViewData["searchDiaChi"] = searchDiaChi;
            ViewData["searchAoNuoi"] = searchAoNuoi;
            ViewData["filterDuNo"] = filterDuNo;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ma_desc" : "";
            ViewData["TenSortParm"] = sortOrder == "ten_asc" ? "ten_desc" : "ten_asc";
            ViewData["SdtSortParm"] = sortOrder == "sdt_asc" ? "sdt_desc" : "sdt_asc";
            ViewData["DcSortParm"] = sortOrder == "dc_asc" ? "dc_desc" : "dc_asc";
            ViewData["AoSortParm"] = sortOrder == "ao_asc" ? "ao_desc" : "ao_asc";
            ViewData["DuNoSortParm"] = sortOrder == "duno_asc" ? "duno_desc" : "duno_asc";

            query = sortOrder switch
            {
                "ma_desc" => query.OrderByDescending(k => k.MaDoiTuong),
                "ten_asc" => query.OrderBy(k => k.TenDoiTuong),
                "ten_desc" => query.OrderByDescending(k => k.TenDoiTuong),
                "sdt_asc" => query.OrderBy(k => k.SoDienThoai),
                "sdt_desc" => query.OrderByDescending(k => k.SoDienThoai),
                "dc_asc" => query.OrderBy(k => k.DiaChi),
                "dc_desc" => query.OrderByDescending(k => k.DiaChi),
                "ao_asc" => query.OrderBy(k => k.AoNuoi),
                "ao_desc" => query.OrderByDescending(k => k.AoNuoi),
                "duno_asc" => query.OrderBy(k => k.DuNoLuyKe),
                "duno_desc" => query.OrderByDescending(k => k.DuNoLuyKe),
                _ => query.OrderBy(k => k.MaDoiTuong)
            };

            return View(await query.ToListAsync());
        }

        // GET: PrivateLedger/Details/KH001 - View ledger entries for a specific customer
        [HttpGet("/PrivateLedger/Details/{id?}")]
        public async Task<IActionResult> Details(
            string? id,
            string? pxMaPhieu, DateTime? pxFromDate, DateTime? pxToDate, string? pxTrangThai, string? pxSortOrder,
            DateTime? entryFromDate, DateTime? entryToDate, string? entryLoai, string? entryDienGiai, string? entrySortOrder,
            string? dsMaPhieu, DateTime? dsFromDate, DateTime? dsToDate, string? dsStatus, string? dsSortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.MaDoiTuong == id);

            if (customer == null)
            {
                return NotFound();
            }

            // 1. Initial Load
            var entries = await _context.SoRiengKhachHangs
                .Where(e => e.MaKhachHang == id)
                .OrderByDescending(e => e.NgayGiaoDich)
                .ToListAsync();

            var phieuXuats = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                    .ThenInclude(c => c.MaHangNavigation)
                .Where(p => p.IdKhachHang == id)
                .OrderByDescending(p => p.NgayXuat)
                .ToListAsync();

            // 2. Auto-correct logic (Keep existing)
            bool needsSave = false;
            foreach (var px in phieuXuats.Where(p => p.LoaiXuat == "SALES"))
            {
                bool hasEntry = entries.Count > 0 && entries.Any(e => e.LoaiGiaoDich == "MUA_HANG" && 
                                               (e.SoTienPhatSinh == px.SoPhaiThanhToan && e.NgayGiaoDich?.Date == px.NgayXuat?.Date) ||
                                               (e.DienGiai != null && e.DienGiai.Contains(px.MaPhieu)));
                if (!hasEntry)
                {
                    var newEntry = new SoRiengKhachHang
                    {
                        MaKhachHang = id,
                        NgayGiaoDich = px.NgayXuat ?? DateTime.Now,
                        LoaiGiaoDich = "MUA_HANG",
                        SoTienPhatSinh = px.SoPhaiThanhToan,
                        DienGiai = $"Tự động ghi nợ xuất bán hàng {px.MaPhieu} (Auto-recovered)"
                    };
                    _context.SoRiengKhachHangs.Add(newEntry);
                    entries.Add(newEntry);
                    needsSave = true;

                    if (px.TrangThaiThanhToan == "Đã Thanh Toán")
                    {
                        var paymentEntry = new SoRiengKhachHang
                        {
                            MaKhachHang = id,
                            NgayGiaoDich = px.NgayXuat ?? DateTime.Now,
                            LoaiGiaoDich = "THANH_TOAN",
                            SoTienPhatSinh = px.SoPhaiThanhToan,
                            DienGiai = $"Thanh toán ngay cho phiếu {px.MaPhieu} (Auto-recovered)"
                        };
                        _context.SoRiengKhachHangs.Add(paymentEntry);
                        entries.Add(paymentEntry);
                    }
                }
            }

            foreach (var entry in entries)
            {
                if (entry.SoTienPhatSinh < 0)
                {
                    entry.SoTienPhatSinh = Math.Abs(entry.SoTienPhatSinh.Value);
                    _context.Update(entry);
                    needsSave = true;
                }
                if (entry.LoaiGiaoDich == "Mua Hàng")
                {
                    entry.LoaiGiaoDich = "MUA_HANG";
                    _context.Update(entry);
                    needsSave = true;
                }
            }

            if (needsSave || (customer.DuNoLuyKe ?? 0) < 0 || entries.Count > 0)
            {
                decimal calculatedDebt = 0;
                foreach (var entry in entries.OrderBy(e => e.NgayGiaoDich))
                {
                    if (entry.LoaiGiaoDich == "MUA_HANG")
                        calculatedDebt += entry.SoTienPhatSinh ?? 0;
                    else if (entry.LoaiGiaoDich == "THANH_TOAN" || entry.LoaiGiaoDich == "CAN_TRU" || entry.LoaiGiaoDich == "CHI_CK")
                        calculatedDebt -= entry.SoTienPhatSinh ?? 0;
                }
                
                if (customer.DuNoLuyKe != calculatedDebt)
                {
                    customer.DuNoLuyKe = calculatedDebt;
                    _context.Update(customer);
                    needsSave = true;
                }
            }

            if (needsSave)
            {
                await _context.SaveChangesAsync();
                // Refresh lists after save
                entries = await _context.SoRiengKhachHangs.Where(e => e.MaKhachHang == id).ToListAsync();
                phieuXuats = await _context.PhieuXuats
                    .Include(p => p.ChiTietPhieuXuats).ThenInclude(c => c.MaHangNavigation)
                    .Where(p => p.IdKhachHang == id).ToListAsync();
            }

            // 3. Apply Filters and Sorting to Purchase History (phieuXuats)
            if (!string.IsNullOrEmpty(pxMaPhieu)) phieuXuats = [.. phieuXuats.Where(p => p.MaPhieu != null && p.MaPhieu.Contains(pxMaPhieu))];
            if (pxFromDate.HasValue) phieuXuats = [.. phieuXuats.Where(p => p.NgayXuat?.Date >= pxFromDate.Value.Date)];
            if (pxToDate.HasValue) phieuXuats = [.. phieuXuats.Where(p => p.NgayXuat?.Date <= pxToDate.Value.Date)];
            if (!string.IsNullOrEmpty(pxTrangThai)) phieuXuats = [.. phieuXuats.Where(p => p.TrangThaiThanhToan == pxTrangThai)];

            ViewData["PXSort_Ma"] = pxSortOrder == "ma_asc" ? "ma_desc" : "ma_asc";
            ViewData["PXSort_Date"] = string.IsNullOrEmpty(pxSortOrder) || pxSortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["PXSort_Status"] = pxSortOrder == "status_asc" ? "status_desc" : "status_asc";
            ViewData["PXSort_Total"] = pxSortOrder == "total_asc" ? "total_desc" : "total_asc";
            ViewData["PXSort_Paid"] = pxSortOrder == "paid_asc" ? "paid_desc" : "paid_asc";
            ViewData["PXSort_Debt"] = pxSortOrder == "debt_asc" ? "debt_desc" : "debt_asc";

            phieuXuats = pxSortOrder switch {
                "ma_asc" => [.. phieuXuats.OrderBy(p => p.MaPhieu)],
                "ma_desc" => [.. phieuXuats.OrderByDescending(p => p.MaPhieu)],
                "date_asc" => [.. phieuXuats.OrderBy(p => p.NgayXuat)],
                "date_desc" => [.. phieuXuats.OrderByDescending(p => p.NgayXuat)],
                "status_asc" => [.. phieuXuats.OrderBy(p => p.TrangThaiThanhToan)],
                "status_desc" => [.. phieuXuats.OrderByDescending(p => p.TrangThaiThanhToan)],
                "total_asc" => [.. phieuXuats.OrderBy(p => p.SoPhaiThanhToan)],
                "total_desc" => [.. phieuXuats.OrderByDescending(p => p.SoPhaiThanhToan)],
                "paid_asc" => [.. phieuXuats.OrderBy(p => p.SoDaThanhToan)],
                "paid_desc" => [.. phieuXuats.OrderByDescending(p => p.SoDaThanhToan)],
                "debt_asc" => [.. phieuXuats.OrderBy(p => p.SoChuaThanhToan)],
                "debt_desc" => [.. phieuXuats.OrderByDescending(p => p.SoChuaThanhToan)],
                _ => [.. phieuXuats.OrderByDescending(p => p.NgayXuat)]
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
                .Where(p => p.MaDoiTuong == id && p.LoaiDoiTuong == "KHACH")
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

            ViewData["pxMaPhieu"] = pxMaPhieu;
            ViewData["pxFromDate"] = pxFromDate?.ToString("yyyy-MM-dd");
            ViewData["pxToDate"] = pxToDate?.ToString("yyyy-MM-dd");
            ViewData["pxTrangThai"] = pxTrangThai;
            ViewData["pxSortOrder"] = pxSortOrder;

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
            ViewBag.PhieuXuats = phieuXuats;
            ViewBag.Discounts = discounts;
            return View(customer);
        }

        // POST: PrivateLedger/PayDiscount - Pay for a discount voucher (CHI)
        [HttpPost("/PrivateLedger/PayDiscount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDiscount(string maKhachHang, string maPhieuTinh, decimal amount, string? dienGiai)
        {
            if (amount <= 0)
            {
                return RedirectToAction(nameof(Details), new { id = maKhachHang });
            }

            var phieu = await _context.PhieuTinhChietKhaus.FindAsync(maPhieuTinh);
            if (phieu == null) return NotFound();

            // 1. Create PhieuThuChi (CHI)
            var paymentVoucher = new PhieuThuChi
            {
                MaPhieu = "PC_" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                LoaiPhieu = "CHI CHIET KHAU KHACH HANG",
                NgayLap = DateTime.Now,
                SoTien = amount,
                LyDo = string.IsNullOrEmpty(dienGiai) ? $"Chi tiền chiết khấu cho phiếu {maPhieuTinh}" : dienGiai,
                MaDoiTuong = maKhachHang,
                LoaiDoiTuong = "KH"
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
            
            // 3. Add to Customer Ledger (SoRiengKhachHang) - This reduces debt
            var ledgerEntry = new SoRiengKhachHang
            {
                MaKhachHang = maKhachHang,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = "CHI_CK",
                SoTienPhatSinh = amount,
                DienGiai = string.IsNullOrEmpty(dienGiai) ? $"Chi trả chiết khấu cho phiếu {maPhieuTinh}" : dienGiai
            };
            _context.SoRiengKhachHangs.Add(ledgerEntry);

            // 4. Update Customer Cumulative Debt
            var customer = await _context.KhachHangs.FindAsync(maKhachHang);
            if (customer != null)
            {
                customer.DuNoLuyKe = (customer.DuNoLuyKe ?? 0) - amount;
                _context.Update(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = maKhachHang });
        }

        // GET: PrivateLedger/Create
        [HttpGet("/PrivateLedger/Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrivateLedger/Create - Create a new customer
        [HttpPost("/PrivateLedger/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDoiTuong,TenDoiTuong,SoDienThoai,DiaChi,AoNuoi")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                khachHang.DuNoLuyKe = 0;
                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // POST: PrivateLedger/AddEntry - Add a ledger entry for a customer
        [HttpPost("/PrivateLedger/AddEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(string maKhachHang, string loaiGiaoDich, decimal soTienPhatSinh, string? dienGiai)
        {
            // Ensure soTienPhatSinh is always positive
            soTienPhatSinh = Math.Abs(soTienPhatSinh);

            var ledgerEntry = new SoRiengKhachHang
            {
                MaKhachHang = maKhachHang,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = loaiGiaoDich,
                SoTienPhatSinh = soTienPhatSinh,
                DienGiai = dienGiai
            };

            _context.Add(ledgerEntry);

            // Update customer cumulative debt
            var customer = await _context.KhachHangs.FindAsync(maKhachHang);
            if (customer != null)
            {
                // MUA_HANG creates debt (+); THANH_TOAN/CAN_TRU clears debt (-)
                if (loaiGiaoDich == "MUA_HANG")
                {
                    customer.DuNoLuyKe = (customer.DuNoLuyKe ?? 0) + soTienPhatSinh;
                }
                else if (loaiGiaoDich == "THANH_TOAN" || loaiGiaoDich == "CAN_TRU" || loaiGiaoDich == "CHI_CK")
                {
                    customer.DuNoLuyKe = (customer.DuNoLuyKe ?? 0) - soTienPhatSinh;

                    // Create PhieuThuChi (THU) if it's a payment
                    if (loaiGiaoDich == "THANH_TOAN")
                    {
                        var paymentVoucher = new PhieuThuChi
                        {
                            MaPhieu = "PT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                            LoaiPhieu = "THU BAN HANG",
                            NgayLap = DateTime.Now,
                            SoTien = soTienPhatSinh,
                            LyDo = string.IsNullOrEmpty(dienGiai) ? "Thu tiền thanh toán từ khách hàng " + maKhachHang : dienGiai,
                            MaDoiTuong = maKhachHang,
                            LoaiDoiTuong = "KH"
                        };
                        _context.PhieuThuChis.Add(paymentVoucher);
                    }

                    // Distribute payment to unpaid PhieuXuats
                    var unpaidInvoices = await _context.PhieuXuats
                        .Where(p => p.IdKhachHang == maKhachHang && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán")
                        .OrderBy(p => p.NgayXuat)
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
                            if (inv.SoChuaThanhToan > 0)
                            {
                                inv.TrangThaiThanhToan = "Thanh Toán Một Phần";
                            }
                        }
                        _context.Update(inv);
                    }
                }
                _context.Update(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = maKhachHang });
        }
        // POST: PrivateLedger/PayBill - Pay for a specific bill (partially or fully)
        [HttpPost("/PrivateLedger/PayBill")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBill(string maKhachHang, string maPhieu, decimal amount, string? dienGiai)
        {
            if (amount <= 0)
            {
                return RedirectToAction(nameof(Details), new { id = maKhachHang });
            }

            // 1. Create a ledger entry for the payment
            var ledgerEntry = new SoRiengKhachHang
            {
                MaKhachHang = maKhachHang,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = "THANH_TOAN",
                SoTienPhatSinh = amount,
                DienGiai = string.IsNullOrEmpty(dienGiai) ? $"Thanh toán for phiếu {maPhieu}" : dienGiai
            };
            _context.Add(ledgerEntry);

            // 2. Update customer cumulative debt
            var customer = await _context.KhachHangs.FindAsync(maKhachHang);
            if (customer != null)
            {
                customer.DuNoLuyKe = (customer.DuNoLuyKe ?? 0) - amount;
                _context.Update(customer);

                // 2b. Create PhieuThuChi (THU) for cash flow
                var paymentVoucher = new PhieuThuChi
                {
                    MaPhieu = "PT_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    LoaiPhieu = "THU BAN HANG",
                    NgayLap = DateTime.Now,
                    SoTien = amount,
                    LyDo = string.IsNullOrEmpty(dienGiai) ? $"Thu tiền thanh toán for phiếu {maPhieu}" : dienGiai,
                    MaDoiTuong = maKhachHang,
                    LoaiDoiTuong = "KH"
                };
                _context.PhieuThuChis.Add(paymentVoucher);

                // 3. Distribute payment to unpaid PhieuXuats using FIFO logic
                // This maintains system consistency as requested in previous logic requirements.
                var unpaidInvoices = await _context.PhieuXuats
                    .Where(p => p.IdKhachHang == maKhachHang && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán")
                    .OrderBy(p => p.NgayXuat)
                    .ToListAsync();
                
                decimal remainingPayment = amount;
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
                        if (inv.SoChuaThanhToan > 0)
                        {
                            inv.TrangThaiThanhToan = "Thanh Toán Một Phần";
                        }
                    }
                    _context.Update(inv);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = maKhachHang });
        }
        // GET: PrivateLedger/Print
        [HttpGet("/PrivateLedger/Print")]
        public async Task<IActionResult> Print(string id, int[] selectedEntries, string[] selectedBills, string[] selectedDiscounts)
        {
            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer == null) return NotFound();

            var entries = await _context.SoRiengKhachHangs
                .Where(e => selectedEntries.Contains(e.Id))
                .OrderBy(e => e.NgayGiaoDich)
                .ToListAsync();

            var phieuXuats = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                    .ThenInclude(c => c.MaHangNavigation)
                .Where(p => selectedBills.Contains(p.MaPhieu))
                .OrderBy(p => p.NgayXuat)
                .ToListAsync();

            var phieuChiets = await _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietChietKhaus)
                    .ThenInclude(c => c.MaHangNavigation)
                .Where(p => selectedDiscounts.Contains(p.MaPhieuTinh))
                .OrderBy(p => p.NgayTao)
                .ToListAsync();

            // 1. Link MUA_HANG entries to PhieuXuats
            foreach (var entry in entries.Where(e => e.LoaiGiaoDich == "MUA_HANG"))
            {
                var words = entry.DienGiai?.Split(' ');
                if (words != null)
                {
                    foreach (var word in words)
                    {
                        if (word.StartsWith("PX") && !phieuXuats.Any(p => p.MaPhieu == word))
                        {
                            var px = await _context.PhieuXuats
                                .Include(p => p.ChiTietPhieuXuats)
                                    .ThenInclude(c => c.MaHangNavigation)
                                .FirstOrDefaultAsync(p => p.MaPhieu == word);
                            if (px != null) phieuXuats.Add(px);
                        }
                    }
                }
            }

            // 2. Link CHI_CK entries to PhieuTinhChietKhaus
            foreach (var entry in entries.Where(e => e.LoaiGiaoDich == "CHI_CK"))
            {
                var words = entry.DienGiai?.Split(' ');
                if (words != null)
                {
                    foreach (var word in words)
                    {
                        if (word.StartsWith("CK") && !phieuChiets.Any(p => p.MaPhieuTinh == word))
                        {
                            var ck = await _context.PhieuTinhChietKhaus
                                .Include(p => p.ChiTietChietKhaus)
                                    .ThenInclude(c => c.MaHangNavigation)
                                .FirstOrDefaultAsync(p => p.MaPhieuTinh == word);
                            if (ck != null) phieuChiets.Add(ck);
                        }
                    }
                }
            }

            ViewBag.Entries = entries;
            ViewBag.PhieuXuats = phieuXuats.OrderBy(p => p.NgayXuat).ToList();
            ViewBag.PhieuChiets = phieuChiets.OrderBy(p => p.NgayTao).ToList();
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string id,
            DateTime? entryFromDate, DateTime? entryToDate, string? entryLoai, string? entryDienGiai)
        {
            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer == null) return NotFound();

            var query = _context.SoRiengKhachHangs.Where(e => e.MaKhachHang == id);

            if (entryFromDate.HasValue) query = query.Where(e => e.NgayGiaoDich >= entryFromDate.Value.Date);
            if (entryToDate.HasValue) query = query.Where(e => e.NgayGiaoDich <= entryToDate.Value.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrEmpty(entryLoai)) query = query.Where(e => e.LoaiGiaoDich == entryLoai);
            if (!string.IsNullOrEmpty(entryDienGiai)) query = query.Where(e => e.DienGiai != null && e.DienGiai.Contains(entryDienGiai));

            var entries = await query.OrderBy(e => e.NgayGiaoDich).ToListAsync();

            var data = entries.Select(e => new {
                Ngay = e.NgayGiaoDich?.ToString("dd/MM/yyyy HH:mm"),
                Loai = e.LoaiGiaoDich == "MUA_HANG" ? "Ghi Nợ (Bán Hàng)" :
                       e.LoaiGiaoDich == "THANH_TOAN" ? "Thanh Toán" :
                       e.LoaiGiaoDich == "CAN_TRU" ? "Cấn Trừ" :
                       e.LoaiGiaoDich == "CHI_CK" ? "Chi Chiết Khấu" : e.LoaiGiaoDich,
                SoTien = e.SoTienPhatSinh,
                DienGiai = e.DienGiai
            }).ToList();

            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(data);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CongNo_KH_{id}.xlsx");
        }
    }
}
