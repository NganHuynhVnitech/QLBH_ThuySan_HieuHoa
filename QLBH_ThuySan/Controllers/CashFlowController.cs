using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Models.ViewModels;
using QLBH_ThuySan.Services;
using System.Text.RegularExpressions;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class CashFlowController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: CashFlow
        public async Task<IActionResult> Index()
        {
            // Pre-load names for display
            ViewData["CustomerNames"] = await _context.KhachHangs.ToDictionaryAsync(k => k.MaDoiTuong, k => k.TenDoiTuong);
            ViewData["SupplierNames"] = await _context.NhaCungCaps.ToDictionaryAsync(n => n.MaDoiTuong, n => n.TenDoiTuong);
            ViewData["CostObjectNames"] = await _context.DoiTuongChiPhis.ToDictionaryAsync(d => d.MaDoiTuong, d => d.TenDoiTuong);

            var model = await _context.PhieuThuChis
                .Where(p => !p.IsDisabled)
                .OrderByDescending(p => p.NgayLap)
                .ToListAsync();

            // Calculate Metrics
            var tongNoKH = await _context.KhachHangs.SumAsync(k => k.DuNoLuyKe ?? 0);
            var tongNoNCC = await _context.NhaCungCaps.SumAsync(n => n.DuNoLuyKe ?? 0);
            var tongNoChietKhauKH = await _context.PhieuTinhChietKhaus.Where(p => p.LoaiDoiTuong == "KHACH" && !p.IsDisabled).SumAsync(p => p.SoChuaThanhToan ?? 0);
            var tongNoChietKhauNCC = await _context.PhieuTinhChietKhaus.Where(p => p.LoaiDoiTuong == "NCC" && !p.IsDisabled).SumAsync(p => p.SoChuaThanhToan ?? 0);

            var tongThuThucTe = model.Where(x => x.LoaiPhieu != null && x.LoaiPhieu.StartsWith("THU")).Sum(x => x.SoTien ?? 0);
            
            // Tong Chi: Anything starting with CHI, or NHAP (historical), or explicitly for CHIPHI objects
            var tongChiThucTe = model.Where(x => x.LoaiPhieu != null && 
                (x.LoaiPhieu.StartsWith("CHI") || x.LoaiPhieu == "NHAP" || x.LoaiDoiTuong == "CHIPHI")).Sum(x => x.SoTien ?? 0);
            
            var thucTeTienMat = tongThuThucTe - tongChiThucTe;

            ViewBag.TongNoKH = tongNoKH;
            ViewBag.TongNoNCC = tongNoNCC;
            ViewBag.TongNoChietKhauKH = tongNoChietKhauKH;
            ViewBag.TongNoChietKhauNCC = tongNoChietKhauNCC;
            ViewBag.TongThuThucTe = tongThuThucTe;
            ViewBag.TongChiThucTe = tongChiThucTe;
            ViewBag.ThucTeTienMat = thucTeTienMat;
            ViewBag.TongThuDuKien = tongThuThucTe + tongNoKH + tongNoChietKhauNCC;
            ViewBag.TongChiDuKien = tongChiThucTe + tongNoNCC + tongNoChietKhauKH;

            return View(model);
        }

        // GET: CashFlow/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var phieuThuChi = await _context.PhieuThuChis.FirstOrDefaultAsync(m => m.MaPhieu == id && !m.IsDisabled);
            if (phieuThuChi == null) return NotFound();

            var viewModel = new CashFlowDetailsViewModel { MainVoucher = phieuThuChi };

            // Load Object Information
            if (phieuThuChi.MaDoiTuong != null)
            {
                if (phieuThuChi.LoaiDoiTuong == "KH" || phieuThuChi.MaDoiTuong.StartsWith("KH"))
                {
                    var kh = await _context.KhachHangs.FindAsync(phieuThuChi.MaDoiTuong);
                    if (kh != null)
                    {
                        viewModel.ObjectName = kh.TenDoiTuong;
                        viewModel.ObjectAddress = kh.DiaChi;
                        viewModel.ObjectPhone = kh.SoDienThoai;
                    }
                }
                else if (phieuThuChi.LoaiDoiTuong == "NCC" || phieuThuChi.MaDoiTuong.StartsWith("NCC"))
                {
                    var ncc = await _context.NhaCungCaps.FindAsync(phieuThuChi.MaDoiTuong);
                    if (ncc != null)
                    {
                        viewModel.ObjectName = ncc.TenDoiTuong;
                        viewModel.ObjectAddress = ncc.DiaChi;
                        viewModel.ObjectPhone = ncc.SoDienThoai;
                    }
                }
                else
                {
                    var cp = await _context.DoiTuongChiPhis.FindAsync(phieuThuChi.MaDoiTuong);
                    viewModel.ObjectName = cp?.TenDoiTuong ?? (phieuThuChi.MaDoiTuong == "KHO_TONG_AO" ? "Kho Tổng Ảo" : phieuThuChi.MaDoiTuong);
                }
            }

            // LINK LOGIC: Extract ID from LyDo or MaPhieu
            string? sourceId = null;
            if (!string.IsNullOrEmpty(phieuThuChi.LyDo))
            {
                var match = Regex.Match(phieuThuChi.LyDo, @"(CK|PN|PX)\d+");
                if (match.Success) sourceId = match.Value;
            }

            // Case 1: Discount Voucher (CK)
            if (phieuThuChi.LoaiPhieu != null && (phieuThuChi.LoaiPhieu.Contains("CHIET KHAU") || (sourceId != null && sourceId.StartsWith("CK"))))
            {
                if (sourceId != null)
                {
                    viewModel.DiscountVoucher = await _context.PhieuTinhChietKhaus
                        .Include(p => p.ChiTietChietKhaus)
                        .ThenInclude(c => c.MaHangNavigation)
                        .FirstOrDefaultAsync(p => p.MaPhieuTinh == sourceId);
                }
            }
            // Case 2: Import Voucher (PN / CHI PHIEU NHAP)
            else if (phieuThuChi.LoaiPhieu == "CHI PHIEU NHAP" || (sourceId != null && sourceId.StartsWith("PN")))
                {
                if (sourceId != null)
                {
                    viewModel.ImportVoucher = await _context.PhieuNhaps
                        .Include(p => p.ChiTietPhieuNhaps)
                        .ThenInclude(c => c.MaHangNavigation)
                        .FirstOrDefaultAsync(p => p.MaPhieu == sourceId);
                }
            }
            // Case 3: Export Voucher (PX / THU BAN HANG)
            else if (phieuThuChi.LoaiPhieu == "THU BAN HANG" || (sourceId != null && sourceId.StartsWith("PX")))
            {
                if (sourceId != null)
                {
                    viewModel.ExportVoucher = await _context.PhieuXuats
                        .Include(p => p.ChiTietPhieuXuats)
                        .ThenInclude(c => c.MaHangNavigation)
                        .FirstOrDefaultAsync(p => p.MaPhieu == sourceId);
                }
            }

            return View(viewModel);
        }

        // GET: CashFlow/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Customers"] = _context.KhachHangs.Select(k => new { k.MaDoiTuong, k.TenDoiTuong }).ToList();
            ViewData["Suppliers"] = _context.NhaCungCaps.Select(n => new { n.MaDoiTuong, n.TenDoiTuong }).ToList();
            ViewData["CostObjects"] = _context.DoiTuongChiPhis.Select(d => new { d.MaDoiTuong, d.TenDoiTuong }).ToList();
            
            var model = new PhieuThuChi
            {
                LoaiPhieu = "THU",
                MaPhieu = await _codeGen.GenerateFinancialCodeAsync("THU"),
                NgayLap = DateTime.Now
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetNextCode(string type)
        {
            var code = await _codeGen.GenerateFinancialCodeAsync(type);
            return Json(new { code = code });
        }

        // POST: CashFlow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieu,LoaiPhieu,NgayLap,SoTien,LyDo,MaDoiTuong")] PhieuThuChi phieuThuChi)
        {
            if (ModelState.IsValid)
            {
                if (_context.PhieuThuChis.Any(e => e.MaPhieu == phieuThuChi.MaPhieu))
                {
                    ModelState.AddModelError("MaPhieu", "Mã phiếu đã tồn tại.");
                    return View(phieuThuChi);
                }

                phieuThuChi.LoaiDoiTuong = phieuThuChi.MaDoiTuong?.StartsWith("KH") == true ? "KH" : (phieuThuChi.MaDoiTuong?.StartsWith("NCC") == true ? "NCC" : "CHIPHI");
                _context.Add(phieuThuChi);

                // NEW PAYMENT DISTRIBUTION LOGIC
                if (!string.IsNullOrEmpty(phieuThuChi.MaDoiTuong) && phieuThuChi.SoTien > 0)
                {
                    if (phieuThuChi.MaDoiTuong.StartsWith("KH") && (phieuThuChi.LoaiPhieu == "THU" || phieuThuChi.LoaiPhieu == "Thu"))
                    {
                        var unpaidInvoices = await _context.PhieuXuats
                            .Where(p => p.IdKhachHang == phieuThuChi.MaDoiTuong && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán")
                            .OrderBy(p => p.NgayXuat)
                            .ToListAsync();
                        
                        decimal remainingPayment = phieuThuChi.SoTien ?? 0;
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
                                inv.NgayThanhToan = phieuThuChi.NgayLap ?? DateTime.Now;
                            }
                            else
                            {
                                inv.SoDaThanhToan = (inv.SoDaThanhToan ?? 0) + remainingPayment;
                                inv.SoChuaThanhToan -= remainingPayment;
                                remainingPayment = 0;
                            }
                            _context.Update(inv);
                        }
                    }
                    else if (phieuThuChi.MaDoiTuong.StartsWith("NCC") && (phieuThuChi.LoaiPhieu == "CHI" || phieuThuChi.LoaiPhieu == "Chi" || phieuThuChi.LoaiPhieu == "NHAP"))
                    {
                        var unpaidInvoices = await _context.PhieuNhaps
                            .Where(p => p.IdNhaCungCap == phieuThuChi.MaDoiTuong && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán")
                            .OrderBy(p => p.NgayNhap)
                            .ToListAsync();
                        
                        decimal remainingPayment = phieuThuChi.SoTien ?? 0;
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
                                inv.NgayThanhToan = phieuThuChi.NgayLap ?? DateTime.Now;
                            }
                            else
                            {
                                inv.SoDaThanhToan = (inv.SoDaThanhToan ?? 0) + remainingPayment;
                                inv.SoChuaThanhToan -= remainingPayment;
                                remainingPayment = 0;
                            }
                            _context.Update(inv);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phieuThuChi);
        }

        // GET: CashFlow/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuThuChis.FindAsync(id);
            if (phieu == null) return NotFound();

            // Guard: Only "CHI PHI" or "NHAP" (Legacy) vouchers can be edited in this view per user request
            if (phieu.LoaiPhieu != "CHI PHI" && phieu.LoaiPhieu != "NHAP")
            {
                return RedirectToAction(nameof(Index));
            }

            ViewData["CostObjects"] = await _context.DoiTuongChiPhis.Where(d => !d.IsDisabled).ToListAsync();
            return View(phieu);
        }

        // POST: CashFlow/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaPhieu,LoaiPhieu,NgayLap,SoTien,LyDo,MaDoiTuong")] PhieuThuChi phieu)
        {
            if (id != phieu.MaPhieu) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    phieu.LoaiDoiTuong = phieu.MaDoiTuong?.StartsWith("KH") == true ? "KH" : (phieu.MaDoiTuong?.StartsWith("NCC") == true ? "NCC" : "CHIPHI");
                    _context.Update(phieu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PhieuThuChis.Any(e => e.MaPhieu == phieu.MaPhieu)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CostObjects"] = await _context.DoiTuongChiPhis.Where(d => !d.IsDisabled).ToListAsync();
            return View(phieu);
        }

        // GET: CashFlow/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
             if (id == null)
            {
                return NotFound();
            }

            var phieuThuChi = await _context.PhieuThuChis
                .FirstOrDefaultAsync(m => m.MaPhieu == id);
            if (phieuThuChi == null)
            {
                return NotFound();
            }

            return View(phieuThuChi);
        }

        // POST: CashFlow/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phieuThuChi = await _context.PhieuThuChis.FindAsync(id);
            if (phieuThuChi != null)
            {
                phieuThuChi.IsDisabled = true;
                _context.Update(phieuThuChi);

                // Reverse Debt Impact
                if (!string.IsNullOrEmpty(phieuThuChi.MaDoiTuong))
                {
                    decimal amount = phieuThuChi.SoTien ?? 0;
                    if ((phieuThuChi.LoaiDoiTuong == "KH" || (phieuThuChi.LoaiPhieu != null && phieuThuChi.LoaiPhieu.Contains("KHACH HANG"))) && !string.IsNullOrEmpty(phieuThuChi.MaDoiTuong))
                    {
                        var kh = await _context.KhachHangs.FindAsync(phieuThuChi.MaDoiTuong);
                        if (kh != null)
                        {
                            if (phieuThuChi.LoaiPhieu != null && phieuThuChi.LoaiPhieu.StartsWith("THU")) kh.DuNoLuyKe += amount;
                            else kh.DuNoLuyKe -= amount;
                        }
                    }
                    else if ((phieuThuChi.LoaiDoiTuong == "NCC" || (phieuThuChi.LoaiPhieu != null && phieuThuChi.LoaiPhieu.Contains("NCC"))) && !string.IsNullOrEmpty(phieuThuChi.MaDoiTuong))
                    {
                        var ncc = await _context.NhaCungCaps.FindAsync(phieuThuChi.MaDoiTuong);
                        if (ncc != null)
                        {
                            if (phieuThuChi.LoaiPhieu == "THU CHIET KHAU NCC") ncc.DuNoLuyKe += amount;
                            else if (phieuThuChi.LoaiPhieu == "THU TRA HANG NCC") ncc.DuNoLuyKe -= amount;
                            else if (phieuThuChi.LoaiPhieu != null && phieuThuChi.LoaiPhieu.StartsWith("CHI")) ncc.DuNoLuyKe += amount;
                            else if (phieuThuChi.LoaiPhieu != null && phieuThuChi.LoaiPhieu.StartsWith("THU")) ncc.DuNoLuyKe -= amount;
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> MigrateVoucherTypes()
        {
            var vouchers = await _context.PhieuThuChis.ToListAsync();
            foreach (var v in vouchers)
            {
                if (v.LoaiPhieu == "THU" || v.LoaiPhieu == "Thu")
                {
                    if (v.LyDo != null && (v.LyDo.ToLower().Contains("thu tiền bán hàng") || v.LyDo.ToLower().Contains("phương px") || v.LyDo.ToLower().Contains("phiếu px") || v.LyDo.ToLower().Contains("khách hàng")))
                        v.LoaiPhieu = "THU BAN HANG";
                    else if (v.LyDo != null && (v.LyDo.ToLower().Contains("chiết khấu") || v.LyDo.ToLower().Contains("ck")))
                        v.LoaiPhieu = "THU CHIET KHAU NCC";
                    else if (v.LyDo != null && (v.LyDo.ToLower().Contains("xuất trả ncc") || v.LyDo.ToLower().Contains("trả hàng")))
                        v.LoaiPhieu = "THU XUAT TRA NCC";
                    else if (v.MaPhieu.StartsWith("PT_") && v.MaDoiTuong != null && v.MaDoiTuong.StartsWith("KH"))
                        v.LoaiPhieu = "THU BAN HANG";
                }
                else if (v.LoaiPhieu == "CHI" || v.LoaiPhieu == "Chi" || v.LoaiPhieu == "NHAP")
                {
                    if (v.LyDo != null && (v.LyDo.ToLower().Contains("chiết khấu") || v.LyDo.ToLower().Contains("ck")))
                        v.LoaiPhieu = "CHI CHIET KHAU KHACH HANG";
                    else if (v.LyDo != null && (v.LyDo.ToLower().Contains("phiếu nhập") || v.LyDo.ToLower().Contains("thanh toán cho phiếu pn") || v.LyDo.ToLower().Contains("phiếu pn")))
                        v.LoaiPhieu = "CHI PHIEU NHAP";
                    else if (v.LyDo != null && (v.LyDo.ToLower().Contains("điện") || v.LyDo.ToLower().Contains("nước") || v.LyDo.ToLower().Contains("lương") || v.LyDo.ToLower().Contains("xăng") || v.LyDo.ToLower().Contains("xe") || v.LyDo.ToLower().Contains("chi phí")))
                        v.LoaiPhieu = "CHI PHI";
                    else if (v.LoaiPhieu == "NHAP")
                         v.LoaiPhieu = "CHI PHI";
                }
                v.LoaiDoiTuong = v.MaDoiTuong?.StartsWith("KH") == true ? "KH" : (v.MaDoiTuong?.StartsWith("NCC") == true ? "NCC" : "CHIPHI");
                _context.Update(v);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncDebt()
        {
            // 1. Reset all cumulative debts to 0
            var customers = await _context.KhachHangs.ToListAsync();
            foreach (var kh in customers) kh.DuNoLuyKe = 0;

            var suppliers = await _context.NhaCungCaps.ToListAsync();
            foreach (var ncc in suppliers) ncc.DuNoLuyKe = 0;

            // 2. Process all Export slips (SALES -> +, RETURN -> -)
            var exports = await _context.PhieuXuats.Where(p => !p.IsDisabled).ToListAsync();
            foreach (var px in exports)
            {
                if (px.LoaiXuat == "SALES" && !string.IsNullOrEmpty(px.IdKhachHang))
                {
                    var target = customers.FirstOrDefault(c => c.MaDoiTuong == px.IdKhachHang);
                    if (target != null) target.DuNoLuyKe += (px.SoPhaiThanhToan ?? 0);
                }
                else if (px.LoaiXuat == "RETURN_VENDOR" && !string.IsNullOrEmpty(px.IdNhaCungCap))
                {
                    var target = suppliers.FirstOrDefault(s => s.MaDoiTuong == px.IdNhaCungCap);
                    if (target != null) target.DuNoLuyKe -= (px.SoPhaiThanhToan ?? 0);
                }
            }

            // 3. Process all Import slips (PN -> +)
            var imports = await _context.PhieuNhaps.Where(p => !p.IsDisabled).ToListAsync();
            foreach (var pn in imports)
            {
                if (!string.IsNullOrEmpty(pn.IdNhaCungCap))
                {
                    var target = suppliers.FirstOrDefault(s => s.MaDoiTuong == pn.IdNhaCungCap);
                    if (target != null) target.DuNoLuyKe += (pn.SoPhaiThanhToan ?? 0);
                }
            }

            // 4. Process all Discount slips (KHACH -> -, NCC -> -)
            var discounts = await _context.PhieuTinhChietKhaus.Where(p => !p.IsDisabled).ToListAsync();
            foreach (var ck in discounts)
            {
                if (ck.LoaiDoiTuong == "KHACH" && !string.IsNullOrEmpty(ck.MaDoiTuong))
                {
                    var target = customers.FirstOrDefault(c => c.MaDoiTuong == ck.MaDoiTuong);
                    if (target != null) target.DuNoLuyKe -= (ck.SoPhaiThanhToan ?? 0);
                }
                else if (ck.LoaiDoiTuong == "NCC" && !string.IsNullOrEmpty(ck.MaDoiTuong))
                {
                    var target = suppliers.FirstOrDefault(s => s.MaDoiTuong == ck.MaDoiTuong);
                    if (target != null) target.DuNoLuyKe -= (ck.SoPhaiThanhToan ?? 0);
                }
            }

            // 5. Process all Cash Vouchers (THU -> -, CHI -> -)
            var vouchers = await _context.PhieuThuChis.Where(p => !p.IsDisabled).ToListAsync();
            foreach (var v in vouchers)
            {
                if ((v.LoaiDoiTuong == "KH" || (v.LoaiPhieu != null && v.LoaiPhieu.Contains("KHACH HANG"))) && !string.IsNullOrEmpty(v.MaDoiTuong))
                {
                    var target = customers.FirstOrDefault(c => c.MaDoiTuong == v.MaDoiTuong);
                    if (target != null)
                    {
                        if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("THU")) target.DuNoLuyKe -= (v.SoTien ?? 0);
                        else target.DuNoLuyKe += (v.SoTien ?? 0);
                    }
                }
                else if ((v.LoaiDoiTuong == "NCC" || (v.LoaiPhieu != null && v.LoaiPhieu.Contains("NCC"))) && !string.IsNullOrEmpty(v.MaDoiTuong))
                {
                    var target = suppliers.FirstOrDefault(s => s.MaDoiTuong == v.MaDoiTuong);
                    if (target != null)
                    {
                        if (v.LoaiPhieu == "THU CHIET KHAU NCC") target.DuNoLuyKe -= (v.SoTien ?? 0);
                        else if (v.LoaiPhieu == "THU TRA HANG NCC") target.DuNoLuyKe += (v.SoTien ?? 0);
                        else if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("CHI")) target.DuNoLuyKe -= (v.SoTien ?? 0);
                        else if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("THU")) target.DuNoLuyKe += (v.SoTien ?? 0);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã đồng bộ lại toàn bộ số dư công nợ khách hàng và nhà cung cấp thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
