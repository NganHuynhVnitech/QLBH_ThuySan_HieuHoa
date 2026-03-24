using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Import
        public async Task<IActionResult> Index()
        {
            var imports = await _context.PhieuNhaps
                .Include(p => p.IdDaiLyNhapNavigation)
                .Include(p => p.IdNhaCungCapNavigation)
                .Where(p => !p.IsDisabled)
                .OrderByDescending(p => p.NgayNhap)
                .ToListAsync();
            
            ViewData["IdDaiLyNhap"] = new SelectList(_context.DaiLys.Where(d => !d.IsDisabled), "MaDaiLy", "TenDaiLy");
            ViewData["IdNhaCungCap"] = new SelectList(_context.NhaCungCaps.Where(n => !n.IsDisabled), "MaDoiTuong", "TenDoiTuong");
            
            return View(imports);
        }

        // GET: Import/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.IdDaiLyNhapNavigation)
                .Include(p => p.IdNhaCungCapNavigation)
                .Include(p => p.ChiTietPhieuNhaps)
                .ThenInclude(ct => ct.MaHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhieu == id);

            if (phieuNhap == null)
            {
                return NotFound();
            }

            // Check if there are older unpaid bills for this supplier
            ViewBag.HasOlderUnpaid = await _context.PhieuNhaps
                .AnyAsync(p => p.IdNhaCungCap == phieuNhap.IdNhaCungCap 
                          && p.NgayNhap < phieuNhap.NgayNhap 
                          && p.SoChuaThanhToan > 0
                          && p.TrangThaiThanhToan != "Đã Thanh Toán"
                          && !p.IsDisabled);

            return View(phieuNhap);
        }

        // GET: Import/Create
        public IActionResult Create()
        {
            ViewData["IdDaiLyNhap"] = new SelectList(_context.DaiLys.Where(d => d.LoaiDaiLy != "BAN_C"), "MaDaiLy", "TenDaiLy");
            ViewData["IdNhaCungCap"] = new SelectList(_context.NhaCungCaps, "MaDoiTuong", "TenDoiTuong");
            ViewData["HangHoaList"] =  _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.DonViTinh }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieu,NgayNhap,IdDaiLyNhap,IdNhaCungCap,TrangThaiThanhToan,SoPhaiThanhToan,SoDaThanhToan,SoChuaThanhToan,HanThanhToan")] PhieuNhap phieuNhap, string[] MaHang, double[] SoLuong, decimal[] DonGiaNhap)
        {
            if (ModelState.IsValid)
            {
                // Check duplicate ID
                if (_context.PhieuNhaps.Any(e => e.MaPhieu == phieuNhap.MaPhieu))
                {
                    ModelState.AddModelError("MaPhieu", "Mã phiếu đã tồn tại.");
                    // Reload Data
                    ViewData["IdDaiLyNhap"] = new SelectList(_context.DaiLys.Where(d => d.LoaiDaiLy != "BAN_C"), "MaDaiLy", "TenDaiLy", phieuNhap.IdDaiLyNhap);
                    ViewData["IdNhaCungCap"] = new SelectList(_context.NhaCungCaps, "MaDoiTuong", "TenDoiTuong", phieuNhap.IdNhaCungCap);
                    ViewData["HangHoaList"] = _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.DonViTinh }).ToList();
                    return View(phieuNhap);
                }

                // Add Details
                if (MaHang != null && MaHang.Length > 0)
                {
                    for (int i = 0; i < MaHang.Length; i++)
                    {
                        var detail = new ChiTietPhieuNhap
                        {
                            MaPhieu = phieuNhap.MaPhieu,
                            MaHang = MaHang[i],
                            SoLuong = SoLuong[i],
                            DonGiaNhap = DonGiaNhap[i]
                        };
                        _context.Add(detail);
                    }
                }

                // Calculate totals
                decimal total = 0;
                if (MaHang != null)
                {
                    for (int i = 0; i < MaHang.Length; i++)
                    {
                        total += (decimal)SoLuong[i] * DonGiaNhap[i];
                    }
                }
                
                phieuNhap.SoPhaiThanhToan = total;
                if (phieuNhap.TrangThaiThanhToan == "Đã Thanh Toán")
                {
                    phieuNhap.SoDaThanhToan = total;
                }
                // SoDaThanhToan for Partial Payment is already bound from the form.
                
                phieuNhap.SoChuaThanhToan = phieuNhap.SoPhaiThanhToan - (phieuNhap.SoDaThanhToan ?? 0);

                _context.Add(phieuNhap);
                await _context.SaveChangesAsync();

                // Call SP to sync inventory and calculate COGS
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_PhieuNhap_DongBoVaTinhGia @p0", phieuNhap.MaPhieu);

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdDaiLyNhap"] = new SelectList(_context.DaiLys.Where(d => d.LoaiDaiLy != "BAN_C"), "MaDaiLy", "TenDaiLy", phieuNhap.IdDaiLyNhap);
            ViewData["IdNhaCungCap"] = new SelectList(_context.NhaCungCaps, "MaDoiTuong", "TenDoiTuong", phieuNhap.IdNhaCungCap);
            ViewData["HangHoaList"] = _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.DonViTinh }).ToList();
            return View(phieuNhap);
        }

        // POST:         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(string id, decimal amount, string dienGiai)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            string contractorId = "";
            PhieuNhap? firstPhieu = null;

            if (id.StartsWith("PN")) // Payment for a specific bill (and others via FIFO)
            {
                firstPhieu = await _context.PhieuNhaps
                    .Include(p => p.IdNhaCungCapNavigation)
                    .FirstOrDefaultAsync(p => p.MaPhieu == id);
                if (firstPhieu == null) return NotFound();
                contractorId = firstPhieu.IdNhaCungCap;
            }
            else // Payment for a supplier directly
            {
                contractorId = id;
            }

            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Số tiền thanh toán không hợp lệ.";
                return (firstPhieu != null) ? RedirectToAction("Details", new { id = firstPhieu.MaPhieu }) : RedirectToAction("Details", "SupplierDebt", new { id = contractorId });
            }

            // 1. Create Payment Voucher (CHI)
            var paymentVoucher = new PhieuThuChi
            {
                MaPhieu = "PT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                LoaiPhieu = "CHI",
                NgayLap = DateTime.Now,
                SoTien = amount,
                LyDo = string.IsNullOrEmpty(dienGiai) ? (firstPhieu != null ? "Chi tiền thanh toán phiếu " + firstPhieu.MaPhieu : "Chi tiền trả nợ nhà cung cấp") : dienGiai,
                MaDoiTuong = contractorId
            };
            _context.PhieuThuChis.Add(paymentVoucher);

            // 2. Update supplier cumulative debt
            var ncc = await _context.NhaCungCaps.FindAsync(contractorId);
            if (ncc != null)
            {
                ncc.DuNoLuyKe = (ncc.DuNoLuyKe ?? 0) - amount;
                _context.Update(ncc);
            }

            // 3. FIFO distribution to unpaid PhieuNhaps
            var unpaidInvoices = await _context.PhieuNhaps
                .Where(p => p.IdNhaCungCap == contractorId && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán" && !p.IsDisabled)
                .OrderBy(p => p.NgayNhap)
                .ToListAsync();

            decimal remainingPayment = amount;
            var settledBills = new List<string>();
            foreach (var inv in unpaidInvoices)
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
                    settledBills.Add(inv.MaPhieu);
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
                    settledBills.Add(inv.MaPhieu + " (một phần)");
                }
                _context.Update(inv);
            }

            await _context.SaveChangesAsync();
            string billList = string.Join(", ", settledBills);
            TempData["SuccessMessage"] = $"Đã xác nhận chi {amount:N0} VNĐ cho {ncc?.TenDoiTuong}. Tiền được phân bổ cho: {billList}.";
            
            if (firstPhieu != null) return RedirectToAction("Details", new { id = firstPhieu.MaPhieu });
            return RedirectToAction("Details", "SupplierDebt", new { id = contractorId });
        }

        // POST: Import/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phieuNhap = await _context.PhieuNhaps.FindAsync(id);
            if (phieuNhap != null)
            {
                phieuNhap.IsDisabled = true;
                _context.Update(phieuNhap);
                await _context.SaveChangesAsync();
                
                // Note: We might also want to revert inventory/COGS if the business logic requires it.
                // For now, consistent with other modules, we just mark as disabled.
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> SearchJson(
            string term, string sortColumn, string sortOrder, 
            string maPhieu, string idDaiLy, string idNhaCungCap, string trangThai,
            string ngayNhapTu, string ngayNhapDen,
            string hanThanhToanTu, string hanThanhToanDen,
            string ngayThanhToanTu, string ngayThanhToanDen,
            bool hanThanhToan3Ngay)
        {
            var query = _context.PhieuNhaps
                .Include(p => p.IdDaiLyNhapNavigation)
                .Include(p => p.IdNhaCungCapNavigation)
                .Where(p => !p.IsDisabled);

            // Filtering
            if (!string.IsNullOrEmpty(maPhieu)) query = query.Where(p => p.MaPhieu.Contains(maPhieu));
            if (!string.IsNullOrEmpty(idDaiLy)) query = query.Where(p => p.IdDaiLyNhap == idDaiLy);
            if (!string.IsNullOrEmpty(idNhaCungCap)) query = query.Where(p => p.IdNhaCungCap == idNhaCungCap);
            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == "Chưa Thanh Toán")
                {
                    query = query.Where(p => p.SoChuaThanhToan > 0);
                }
                else if (trangThai == "Đã Thanh Toán")
                {
                    query = query.Where(p => p.SoChuaThanhToan <= 0);
                }
                else
                {
                    query = query.Where(p => p.TrangThaiThanhToan == trangThai);
                }
            }

            // Ngày Nhập
            if (!string.IsNullOrEmpty(ngayNhapTu) && DateTime.TryParse(ngayNhapTu, out DateTime nnTu))
                query = query.Where(p => p.NgayNhap >= nnTu.Date);
            if (!string.IsNullOrEmpty(ngayNhapDen) && DateTime.TryParse(ngayNhapDen, out DateTime nnDen))
            {
                var den = nnDen.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.NgayNhap <= den);
            }

            // Hạn Thanh Toán
            if (!string.IsNullOrEmpty(hanThanhToanTu) && DateTime.TryParse(hanThanhToanTu, out DateTime httTu))
                query = query.Where(p => p.HanThanhToan >= httTu.Date);
            if (!string.IsNullOrEmpty(hanThanhToanDen) && DateTime.TryParse(hanThanhToanDen, out DateTime httDen))
            {
                var den = httDen.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.HanThanhToan <= den);
            }

            // Ngày Thanh Toán
            if (!string.IsNullOrEmpty(ngayThanhToanTu) && DateTime.TryParse(ngayThanhToanTu, out DateTime nttTu))
                query = query.Where(p => p.NgayThanhToan >= nttTu.Date);
            if (!string.IsNullOrEmpty(ngayThanhToanDen) && DateTime.TryParse(ngayThanhToanDen, out DateTime nttDen))
            {
                var den = nttDen.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.NgayThanhToan <= den);
            }

            var now = DateTime.Now;
            // Hạn thanh toán <= 3 ngày (Trạng thái = Chưa thanh toán, ngày thanh toán= null, hạn thanh toán - today <= 3)
            if (hanThanhToan3Ngay)
            {
                var denNgay = now.Date.AddDays(3).AddDays(1).AddTicks(-1);
                query = query.Where(p => p.TrangThaiThanhToan == "Chưa Thanh Toán" 
                                      && p.NgayThanhToan == null 
                                      && p.HanThanhToan <= denNgay);
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                switch (sortColumn)
                {
                    case "MaPhieu":
                        query = sortOrder == "asc" ? query.OrderBy(p => p.MaPhieu) : query.OrderByDescending(p => p.MaPhieu);
                        break;
                    case "NgayNhap":
                        query = sortOrder == "asc" ? query.OrderBy(p => p.NgayNhap) : query.OrderByDescending(p => p.NgayNhap);
                        break;
                    case "TenDaiLy":
                        query = sortOrder == "asc" ? query.OrderBy(p => p.IdDaiLyNhapNavigation != null ? p.IdDaiLyNhapNavigation.TenDaiLy : "") : query.OrderByDescending(p => p.IdDaiLyNhapNavigation != null ? p.IdDaiLyNhapNavigation.TenDaiLy : "");
                        break;
                    case "TenNhaCungCap":
                        query = sortOrder == "asc" ? query.OrderBy(p => p.IdNhaCungCapNavigation != null ? p.IdNhaCungCapNavigation.TenDoiTuong : "") : query.OrderByDescending(p => p.IdNhaCungCapNavigation != null ? p.IdNhaCungCapNavigation.TenDoiTuong : "");
                        break;
                    case "SoPhaiThanhToan":
                        query = sortOrder == "asc" ? query.OrderBy(p => p.SoPhaiThanhToan) : query.OrderByDescending(p => p.SoPhaiThanhToan);
                        break;
                    case "TrangThai":
                        query = sortOrder == "asc" ? query.OrderBy(p => p.TrangThaiThanhToan) : query.OrderByDescending(p => p.TrangThaiThanhToan);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.NgayNhap);
            }

            var result = await query.Select(p => new
            {
                maPhieu = p.MaPhieu,
                ngayNhap = p.NgayNhap.HasValue ? p.NgayNhap.Value.ToString("dd/MM/yyyy") : "",
                tenDaiLy = p.IdDaiLyNhapNavigation != null ? p.IdDaiLyNhapNavigation.TenDaiLy : "",
                tenNhaCungCap = p.IdNhaCungCapNavigation != null ? p.IdNhaCungCapNavigation.TenDoiTuong : "",
                hanThanhToan = p.HanThanhToan.HasValue ? p.HanThanhToan.Value.ToString("dd/MM/yyyy") : "",
                isQuaHan = p.HanThanhToan.HasValue && p.HanThanhToan.Value < now,
                ngayThanhToan = p.NgayThanhToan.HasValue ? p.NgayThanhToan.Value.ToString("dd/MM/yyyy") : "",
                soPhaiThanhToan = p.SoPhaiThanhToan.HasValue ? p.SoPhaiThanhToan.Value.ToString("N0") : "0",
                soDaThanhToan = p.SoDaThanhToan.HasValue ? p.SoDaThanhToan.Value.ToString("N0") : "0",
                soChuaThanhToan = p.SoChuaThanhToan.HasValue ? p.SoChuaThanhToan.Value.ToString("N0") : "0",
                soChuaThanhToanRaw = p.SoChuaThanhToan ?? 0,
                trangThai = p.TrangThaiThanhToan
            }).ToListAsync();

            return Json(result);
        }
    }
}

