using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;
using System.IO;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class ExportController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: Export Dashboard
        public async Task<IActionResult> Index(
            string type = "", 
            string paymentStatus = "", 
            string targetInfo = "", 
            DateTime? fromDate = null, 
            DateTime? toDate = null,
            string sortOrder = "")
        {
            var query = _context.PhieuXuats
                .Include(p => p.IdDaiLyBanNavigation)
                .Include(p => p.IdKhachHangNavigation)
                .Include(p => p.IdNhaCungCapNavigation)
                .Include(p => p.MaKhoNhanNavigation)
                .Where(p => !p.IsDisabled)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.LoaiXuat == type);
            }
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                if (paymentStatus == "Chưa Thanh Toán")
                {
                    query = query.Where(p => p.TrangThaiThanhToan == "Chưa Thanh Toán" || p.TrangThaiThanhToan == "Thanh Toán Một Phần");
                }
                else
                {
                    query = query.Where(p => p.TrangThaiThanhToan == paymentStatus);
                }
            }
            if (!string.IsNullOrEmpty(targetInfo))
            {
                targetInfo = targetInfo.ToLower();
                query = query.Where(p => 
                    (p.IdKhachHangNavigation != null && p.IdKhachHangNavigation.TenDoiTuong != null && p.IdKhachHangNavigation.TenDoiTuong.ToLower().Contains(targetInfo)) ||
                    (p.IdNhaCungCapNavigation != null && p.IdNhaCungCapNavigation.TenDoiTuong != null && p.IdNhaCungCapNavigation.TenDoiTuong.ToLower().Contains(targetInfo)) ||
                    (p.MaKhoNhanNavigation != null && p.MaKhoNhanNavigation.TenKho != null && p.MaKhoNhanNavigation.TenKho.ToLower().Contains(targetInfo)) ||
                    (p.LyDo != null && p.LyDo.ToLower().Contains(targetInfo)) ||
                    (p.IdDaiLyBanNavigation != null && p.IdDaiLyBanNavigation.TenDaiLy != null && p.IdDaiLyBanNavigation.TenDaiLy.ToLower().Contains(targetInfo))
                );
            }
            if (fromDate.HasValue)
            {
                query = query.Where(p => p.NgayXuat >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(p => p.NgayXuat <= toDate.Value.AddDays(1).AddTicks(-1));
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaPhieuSortParm"] = sortOrder == "MaPhieu" ? "maphieu_desc" : "MaPhieu";
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Date" : (sortOrder == "date_desc" ? "Date" : "date_desc");
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "type_desc" : "Type";
            ViewData["TargetSortParm"] = sortOrder == "Target" ? "target_desc" : "Target";
            ViewData["TotalSortParm"] = sortOrder == "Total" ? "total_desc" : "Total";
            ViewData["PaymentSortParm"] = sortOrder == "Payment" ? "payment_desc" : "Payment";

            switch (sortOrder)
            {
                case "MaPhieu":
                    query = query.OrderBy(p => p.MaPhieu);
                    break;
                case "maphieu_desc":
                    query = query.OrderByDescending(p => p.MaPhieu);
                    break;
                case "Date":
                    query = query.OrderBy(p => p.NgayXuat);
                    break;
                case "Type":
                    query = query.OrderBy(p => p.LoaiXuat);
                    break;
                case "type_desc":
                    query = query.OrderByDescending(p => p.LoaiXuat);
                    break;
                case "Target":
                    query = query.OrderBy(p => 
                        p.LoaiXuat == "SALES" ? p.IdKhachHangNavigation!.TenDoiTuong :
                        p.LoaiXuat == "RETURN_VENDOR" ? p.IdNhaCungCapNavigation!.TenDoiTuong :
                        p.LoaiXuat == "TRANSFER" ? p.MaKhoNhanNavigation!.TenKho : p.LyDo);
                    break;
                case "target_desc":
                    query = query.OrderByDescending(p => 
                        p.LoaiXuat == "SALES" ? p.IdKhachHangNavigation!.TenDoiTuong :
                        p.LoaiXuat == "RETURN_VENDOR" ? p.IdNhaCungCapNavigation!.TenDoiTuong :
                        p.LoaiXuat == "TRANSFER" ? p.MaKhoNhanNavigation!.TenKho : p.LyDo);
                    break;
                case "Total":
                    query = query.OrderBy(p => p.SoPhaiThanhToan);
                    break;
                case "total_desc":
                    query = query.OrderByDescending(p => p.SoPhaiThanhToan);
                    break;
                case "Payment":
                    query = query.OrderBy(p => p.TrangThaiThanhToan);
                    break;
                case "payment_desc":
                    query = query.OrderByDescending(p => p.TrangThaiThanhToan);
                    break;
                default:
                    query = query.OrderByDescending(p => p.NgayXuat);
                    break;
            }

            var exports = await query.ToListAsync();

            ViewData["CurrentType"] = type;
            ViewData["PaymentStatus"] = paymentStatus;
            ViewData["TargetInfo"] = targetInfo;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(exports);
        }

        // GET: Export/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.IdDaiLyBanNavigation)
                .Include(p => p.IdKhachHangNavigation)
                .Include(p => p.IdNhaCungCapNavigation)
                .Include(p => p.MaKhoNhanNavigation)
                .Include(p => p.ChiTietPhieuXuats)
                .ThenInclude(ct => ct.MaHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhieu == id);

            if (phieuXuat == null) return NotFound();
            
            // Check if there are older unpaid bills for this customer
            ViewBag.HasOlderUnpaid = false;
            if (phieuXuat.LoaiXuat == "SALES" && !string.IsNullOrEmpty(phieuXuat.IdKhachHang))
            {
                ViewBag.HasOlderUnpaid = await _context.PhieuXuats
                    .AnyAsync(p => p.IdKhachHang == phieuXuat.IdKhachHang 
                              && p.NgayXuat < phieuXuat.NgayXuat 
                              && p.SoChuaThanhToan > 0
                              && p.TrangThaiThanhToan != "Đã Thanh Toán");
            }

            return View(phieuXuat);
        }

        // GET: Export/Create
        public async Task<IActionResult> Create()
        {
            ViewData["IdDaiLyBan"] = new SelectList(_context.DaiLys.Where(d => d.IsDisabled == false), "MaDaiLy", "TenDaiLy");
            
            var model = new PhieuXuat
            {
                MaPhieu = await _codeGen.GenerateExportCodeAsync(),
                NgayXuat = DateTime.Now,
                TrangThaiThanhToan = "Chưa Thanh Toán"
            };
            return View(model);
        }

        // POST: Export/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuXuat phieuXuat, string[] MaHang, double[] SoLuong, decimal[] GiaBan)
        {
            if (string.IsNullOrEmpty(phieuXuat.IdKhachHang) || MaHang == null || MaHang.Length == 0)
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ (thiếu khách hàng hoặc hàng hóa).");
                ViewData["IdDaiLyBan"] = new SelectList(_context.DaiLys.Where(d => d.IsDisabled == false), "MaDaiLy", "TenDaiLy");
                return View(phieuXuat);
            }

            // Ensure unique MaPhieu if it was generated by POS
            if (await _context.PhieuXuats.AnyAsync(x => x.MaPhieu == phieuXuat.MaPhieu))
            {
                phieuXuat.MaPhieu = "PX" + DateTime.Now.ToString("yyMMddHHmmss"); // Fallback
            }

            phieuXuat.LoaiXuat = "SALES";
            phieuXuat.NgayXuat ??= DateTime.Now;
            if (phieuXuat.TrangThaiThanhToan == "Đã Thanh Toán") phieuXuat.NgayThanhToan = DateTime.Now;

            decimal soPhaiThanhToan = 0;
            List<ChiTietPhieuXuat> details = [];

            for (int i = 0; i < MaHang.Length; i++)
            {
                var hang = await _context.HangHoas.FindAsync(MaHang[i]);
                decimal giaVon = hang?.GiaVonHienTai ?? 0;

                var ct = new ChiTietPhieuXuat
                {
                    MaPhieu = phieuXuat.MaPhieu,
                    MaHang = MaHang[i],
                    SoLuong = SoLuong[i],
                    GiaBan = GiaBan[i],
                    GiaVonTaiThoiDiem = giaVon
                };
                details.Add(ct);
                soPhaiThanhToan += (decimal)SoLuong[i] * GiaBan[i];

                // Inventory deduction logic
                if (hang != null)
                {
                    var ton = await _context.ChiTietTons
                        .Where(t => t.MaHang == MaHang[i] && t.SoLuongTon >= SoLuong[i])
                        .OrderByDescending(t => t.SoLuongTon)
                        .FirstOrDefaultAsync();
                        
                    if (ton == null) ton = await _context.ChiTietTons.FirstOrDefaultAsync(t => t.MaHang == MaHang[i]);
                    
                    if (ton != null)
                    {
                        ton.SoLuongTon -= SoLuong[i];
                    }
                    else
                    {
                        var defaultKho = await _context.Khos.FirstOrDefaultAsync();
                        if (defaultKho != null)
                        {
                            _context.ChiTietTons.Add(new ChiTietTon
                            {
                                MaKho = defaultKho.MaKho,
                                MaHang = MaHang[i],
                                SoLuongTon = -SoLuong[i],
                                GiaTriTon = 0
                            });
                        }
                    }
                }
            }

            phieuXuat.SoPhaiThanhToan = soPhaiThanhToan;
            if (phieuXuat.TrangThaiThanhToan == "Đã Thanh Toán")
            {
                phieuXuat.SoDaThanhToan = soPhaiThanhToan;
            }
            else if (phieuXuat.TrangThaiThanhToan == "Chưa Thanh Toán")
            {
                phieuXuat.SoDaThanhToan = 0;
            }
            // If Partial Payment, SoDaThanhToan is already bound or set by the client.
            
            phieuXuat.SoChuaThanhToan = phieuXuat.SoPhaiThanhToan - (phieuXuat.SoDaThanhToan ?? 0);
            phieuXuat.ChiTietPhieuXuats = details;
            
            _context.PhieuXuats.Add(phieuXuat);
            
            // Sync with Customer Debt if it is a Sales Slip
            if (phieuXuat.LoaiXuat == "SALES" && !string.IsNullOrEmpty(phieuXuat.IdKhachHang))
            {
                var kh = await _context.KhachHangs.FindAsync(phieuXuat.IdKhachHang);
                if (kh != null)
                {
                    _context.SoRiengKhachHangs.Add(new SoRiengKhachHang
                    {
                        MaKhachHang = phieuXuat.IdKhachHang,
                        NgayGiaoDich = DateTime.Now,
                        LoaiGiaoDich = "MUA_HANG",
                        SoTienPhatSinh = soPhaiThanhToan,
                        DienGiai = $"Tự động ghi nợ xuất bán hàng {phieuXuat.MaPhieu}"
                    });

                    if (phieuXuat.TrangThaiThanhToan != "Đã Thanh Toán")
                    {
                        kh.DuNoLuyKe = (kh.DuNoLuyKe ?? 0) + soPhaiThanhToan;
                        _context.Update(kh);
                    }
                    else
                    {
                        // Record the immediate payment to keep the ledger balanced
                        _context.SoRiengKhachHangs.Add(new SoRiengKhachHang
                        {
                            MaKhachHang = phieuXuat.IdKhachHang,
                            NgayGiaoDich = DateTime.Now,
                            LoaiGiaoDich = "THANH_TOAN",
                            SoTienPhatSinh = soPhaiThanhToan,
                            DienGiai = $"Thanh toán ngay cho phiếu {phieuXuat.MaPhieu}"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Export/CreateSlip
        public IActionResult CreateSlip()
        {
            // Prepare dropdown data for the frontend Vue/JS
            ViewData["Khos"] = _context.Khos.Select(k => new { k.MaKho, k.TenKho }).ToList();
            ViewData["HangHoas"] = _context.HangHoas
                .Include(h => h.ChiTietTons)
                .Select(h => new { 
                    h.MaHang, 
                    h.TenHang, 
                    h.GiaBanHienTai, 
                    h.GiaVonHienTai,
                    h.DonViTinh,
                    TonKho = h.ChiTietTons.Sum(t => t.SoLuongTon ?? 0)
                }).ToList();
            ViewData["NhaCungCaps"] = _context.NhaCungCaps.Select(n => new { n.MaDoiTuong, n.TenDoiTuong }).ToList();

            return View();
        }

        [HttpGet("api/inventory/vendor-products")]
        public async Task<IActionResult> GetVendorProducts(string vendorId, string? warehouseCode = null)
        {
            if (string.IsNullOrEmpty(vendorId))
                return BadRequest(new { message = "VendorId is required" });

            var products = await _context.PhieuNhaps
                .Where(p => p.IdNhaCungCap == vendorId)
                .SelectMany(p => p.ChiTietPhieuNhaps)
                .Select(ct => ct.MaHangNavigation)
                .Distinct()
                .Select(h => new {
                    h.MaHang,
                    h.TenHang,
                    GiaBanHienTai = h.GiaBanHienTai,
                    GiaVonHienTai = h.GiaVonHienTai,
                    h.DonViTinh,
                    TonKho = string.IsNullOrEmpty(warehouseCode)
                        ? h.ChiTietTons.Sum(t => t.SoLuongTon ?? 0)
                        : h.ChiTietTons.Where(t => t.MaKho == warehouseCode).Sum(t => t.SoLuongTon ?? 0)
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("api/inventory/warehouse-products")]
        public async Task<IActionResult> GetWarehouseProducts(string warehouseCode)
        {
            if (string.IsNullOrEmpty(warehouseCode))
                return BadRequest(new { message = "WarehouseCode is required" });

            // Get products that have stock in this warehouse
            var products = await _context.ChiTietTons
                .Where(t => t.MaKho == warehouseCode && t.SoLuongTon > 0)
                .Select(t => t.MaHangNavigation)
                .Select(h => new {
                    h.MaHang,
                    h.TenHang,
                    h.DonViTinh,
                    TonKho = h.ChiTietTons.Where(t => t.MaKho == warehouseCode).Sum(t => t.SoLuongTon ?? 0),
                    GiaVonHienTai = h.GiaVonHienTai ?? 0,
                    // Get last purchase price
                    GiaNhapGanNhat = _context.ChiTietPhieuNhaps
                        .Where(ct => ct.MaHang == h.MaHang)
                        .OrderByDescending(ct => ct.MaPhieuNavigation.NgayNhap)
                        .Select(ct => ct.DonGiaNhap)
                        .FirstOrDefault() ?? 0
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("api/inventory/product-stock")]
        public async Task<IActionResult> GetProductStock(string maHang, string warehouseCodes)
        {
            if (string.IsNullOrEmpty(maHang) || string.IsNullOrEmpty(warehouseCodes))
                return BadRequest(new { message = "MaHang and WarehouseCodes (comma-separated) are required" });

            var codes = warehouseCodes.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var stocks = await _context.ChiTietTons
                .Where(t => t.MaHang == maHang && codes.Contains(t.MaKho))
                .Select(t => new {
                    t.MaKho,
                    TonKho = t.SoLuongTon ?? 0
                })
                .ToListAsync();

            return Ok(stocks);
        }
        [HttpGet]
        public async Task<JsonResult> SearchJson(
            string type, string paymentStatus, string targetInfo,
            string fromDate, string toDate, string sortOrder)
        {
            var query = _context.PhieuXuats
                .Include(p => p.IdDaiLyBanNavigation)
                .Include(p => p.IdKhachHangNavigation)
                .Include(p => p.IdNhaCungCapNavigation)
                .Include(p => p.MaKhoNhanNavigation)
                .Where(p => !p.IsDisabled)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.LoaiXuat == type);
            }
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                if (paymentStatus == "Chưa Thanh Toán")
                {
                    query = query.Where(p => p.TrangThaiThanhToan == "Chưa Thanh Toán" || p.TrangThaiThanhToan == "Thanh Toán Một Phần");
                }
                else
                {
                    query = query.Where(p => p.TrangThaiThanhToan == paymentStatus);
                }
            }
            if (!string.IsNullOrEmpty(targetInfo))
            {
                var target = targetInfo.ToLower();
                query = query.Where(p => 
                    (p.IdKhachHangNavigation != null && p.IdKhachHangNavigation.TenDoiTuong != null && p.IdKhachHangNavigation.TenDoiTuong.ToLower().Contains(target)) ||
                    (p.IdNhaCungCapNavigation != null && p.IdNhaCungCapNavigation.TenDoiTuong != null && p.IdNhaCungCapNavigation.TenDoiTuong.ToLower().Contains(target)) ||
                    (p.MaKhoNhanNavigation != null && p.MaKhoNhanNavigation.TenKho != null && p.MaKhoNhanNavigation.TenKho.ToLower().Contains(target)) ||
                    (p.LyDo != null && p.LyDo.ToLower().Contains(target)) ||
                    (p.IdDaiLyBanNavigation != null && p.IdDaiLyBanNavigation.TenDaiLy != null && p.IdDaiLyBanNavigation.TenDaiLy.ToLower().Contains(target)) ||
                    p.MaPhieu.Contains(target)
                );
            }
            if (DateTime.TryParse(fromDate, out DateTime fd))
            {
                query = query.Where(p => p.NgayXuat >= fd);
            }
            if (DateTime.TryParse(toDate, out DateTime td))
            {
                query = query.Where(p => p.NgayXuat <= td.AddDays(1).AddTicks(-1));
            }

            switch (sortOrder)
            {
                case "maphieu_desc": query = query.OrderByDescending(p => p.MaPhieu); break;
                case "MaPhieu": query = query.OrderBy(p => p.MaPhieu); break;
                case "date_desc": query = query.OrderByDescending(p => p.NgayXuat); break;
                case "Date": query = query.OrderBy(p => p.NgayXuat); break;
                case "type_desc": query = query.OrderByDescending(p => p.LoaiXuat); break;
                case "Type": query = query.OrderBy(p => p.LoaiXuat); break;
                case "total_desc": query = query.OrderByDescending(p => p.SoPhaiThanhToan); break;
                case "Total": query = query.OrderBy(p => p.SoPhaiThanhToan); break;
                case "payment_desc": query = query.OrderByDescending(p => p.TrangThaiThanhToan); break;
                case "Payment": query = query.OrderBy(p => p.TrangThaiThanhToan); break;
                default: query = query.OrderByDescending(p => p.NgayXuat); break;
            }

            var result = await query.Select(p => new
            {
                maPhieu = p.MaPhieu,
                ngayXuat = p.NgayXuat.HasValue ? p.NgayXuat.Value.ToString("dd/MM/yyyy HH:mm") : "",
                loaiXuat = p.LoaiXuat,
                tenDoiTuong = p.LoaiXuat == "SALES" ? (p.IdKhachHangNavigation != null ? p.IdKhachHangNavigation.TenDoiTuong : "") :
                              p.LoaiXuat == "RETURN_VENDOR" ? (p.IdNhaCungCapNavigation != null ? p.IdNhaCungCapNavigation.TenDoiTuong : "") :
                              p.LoaiXuat == "TRANSFER" ? (p.MaKhoNhanNavigation != null ? p.MaKhoNhanNavigation.TenKho : "") : p.LyDo,
                soPhaiThanhToan = p.SoPhaiThanhToan.HasValue ? p.SoPhaiThanhToan.Value.ToString("N0") : "0",
                soDaThanhToan = p.SoDaThanhToan.HasValue ? p.SoDaThanhToan.Value.ToString("N0") : "0",
                soChuaThanhToan = p.SoChuaThanhToan.HasValue ? p.SoChuaThanhToan.Value.ToString("N0") : "0",
                soChuaThanhToanRaw = p.SoChuaThanhToan ?? 0,
                trangThaiThanhToan = p.TrangThaiThanhToan
            }).ToListAsync();

            return Json(result);
        }

        // POST: Export/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(string id, decimal amount, string dienGiai)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.IdKhachHangNavigation)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieuXuat == null) return NotFound();
            var customerId = phieuXuat.IdKhachHang;

            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = phieuXuat.MaPhieu });
            }

            // 1. Create a ledger entry for the payment (SoRiengKhachHang)
            if (!string.IsNullOrEmpty(customerId))
            {
                var ledgerEntry = new SoRiengKhachHang
                {
                    MaKhachHang = customerId,
                    NgayGiaoDich = DateTime.Now,
                    LoaiGiaoDich = "THANH_TOAN",
                    SoTienPhatSinh = amount,
                    DienGiai = string.IsNullOrEmpty(dienGiai) ? $"Thanh toán cho phiếu {phieuXuat.MaPhieu}" : dienGiai
                };
                _context.SoRiengKhachHangs.Add(ledgerEntry);

                // 2. Update customer cumulative debt
                if (phieuXuat.IdKhachHangNavigation != null)
                {
                    phieuXuat.IdKhachHangNavigation.DuNoLuyKe = (phieuXuat.IdKhachHangNavigation.DuNoLuyKe ?? 0) - amount;
                    _context.Update(phieuXuat.IdKhachHangNavigation);
                }
            }

            // 3. Create Payment Voucher (THU)
            var paymentVoucher = new PhieuThuChi
            {
                MaPhieu = "PT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                LoaiPhieu = phieuXuat.LoaiXuat == "RETURN_VENDOR" ? "THU XUAT TRA NCC" : "THU BAN HANG",
                NgayLap = DateTime.Now,
                SoTien = amount,
                LyDo = string.IsNullOrEmpty(dienGiai) ? ("Thu tiền thanh toán phiếu " + phieuXuat.MaPhieu) : dienGiai,
                MaDoiTuong = customerId,
                LoaiDoiTuong = phieuXuat.LoaiXuat == "RETURN_VENDOR" ? "NCC" : "KH"
            };
            _context.PhieuThuChis.Add(paymentVoucher);

            // 4. Distribute payment to unpaid PhieuXuats using FIFO logic
            var unpaidInvoices = await _context.PhieuXuats
                .Where(p => p.IdKhachHang == customerId && p.SoChuaThanhToan > 0 && p.TrangThaiThanhToan != "Đã Thanh Toán")
                .OrderBy(p => p.NgayXuat)
                .ToListAsync();
            
            decimal remainingPayment = amount;
            List<string> settledBills = [];
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
            TempData["SuccessMessage"] = $"Đã xác nhận thu {amount:N0} VNĐ. Tiền được phân bổ cho: {billList}.";
            return RedirectToAction(nameof(Details), new { id = phieuXuat.MaPhieu });
        }

        // POST: Export/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phieuXuat = await _context.PhieuXuats.FindAsync(id);
            if (phieuXuat != null)
            {
                phieuXuat.IsDisabled = true;
                _context.Update(phieuXuat);

                // Reverse Debt Impact
                if (phieuXuat.LoaiXuat == "SALES" && !string.IsNullOrEmpty(phieuXuat.IdKhachHang))
                {
                    var kh = await _context.KhachHangs.FindAsync(phieuXuat.IdKhachHang);
                    if (kh != null)
                    {
                        // Sales originally increased debt by SoPhai. Deleting it decreases debt by SoPhai.
                        kh.DuNoLuyKe = (kh.DuNoLuyKe ?? 0) - (phieuXuat.SoPhaiThanhToan ?? 0);
                        _context.Update(kh);
                    }
                }
                else if (phieuXuat.LoaiXuat == "RETURN_VENDOR" && !string.IsNullOrEmpty(phieuXuat.IdNhaCungCap))
                {
                    var ncc = await _context.NhaCungCaps.FindAsync(phieuXuat.IdNhaCungCap);
                    if (ncc != null)
                    {
                        // Return originally reduced debt by SoPhai. Deleting it increases debt by SoPhai.
                        ncc.DuNoLuyKe = (ncc.DuNoLuyKe ?? 0) + (phieuXuat.SoPhaiThanhToan ?? 0);
                        _context.Update(ncc);
                    }
                }

                // Find and disable associated PhieuThuChi (vouchers)
                var associatedVouchers = await _context.PhieuThuChis
                    .Where(v => v.LyDo != null && v.LyDo.Contains(phieuXuat.MaPhieu) && !v.IsDisabled)
                    .ToListAsync();

                foreach (var v in associatedVouchers)
                {
                    v.IsDisabled = true;
                    _context.Update(v);

                    // Reverse Debt part from the Voucher
                    decimal amount = v.SoTien ?? 0;
                    if (v.LoaiPhieu != null && (v.LoaiDoiTuong == "KH" || v.LoaiPhieu.Contains("KHACH HANG")) && !string.IsNullOrEmpty(v.MaDoiTuong))
                    {
                        var kh = await _context.KhachHangs.FindAsync(v.MaDoiTuong);
                        if (kh != null)
                        {
                            // If Receipt, deleting it INCREASES debt (Customer owes back)
                            if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("THU")) kh.DuNoLuyKe += amount;
                            // If Payment, deleting it DECREASES debt
                            else kh.DuNoLuyKe -= amount;
                        }
                    }
                    else if (v.LoaiPhieu != null && (v.LoaiDoiTuong == "NCC" || v.LoaiPhieu.Contains("NCC")) && !string.IsNullOrEmpty(v.MaDoiTuong))
                    {
                        var ncc = await _context.NhaCungCaps.FindAsync(v.MaDoiTuong);
                        if (ncc != null)
                        {
                            if (v.LoaiPhieu == "THU TRA HANG NCC") ncc.DuNoLuyKe -= amount; // Receipt from return decreases debt
                            else if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("CHI")) ncc.DuNoLuyKe += amount; // Payment to NCC increases debt
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string id)
        {
            var p = await _context.PhieuXuats
                .Include(x => x.IdKhachHangNavigation)
                .Include(x => x.IdNhaCungCapNavigation)
                .Include(x => x.ChiTietPhieuXuats)
                .ThenInclude(ct => ct.MaHangNavigation)
                .FirstOrDefaultAsync(x => x.MaPhieu == id && !x.IsDisabled);

            if (p == null) return NotFound();

            var details = p.ChiTietPhieuXuats.Select((ct, index) => new {
                STT = index + 1,
                MaHang = ct.MaHang,
                TenHang = ct.MaHangNavigation?.TenHang,
                DVT = ct.MaHangNavigation?.DonViTinh,
                SoLuong = ct.SoLuong,
                GiaBan = ct.GiaBan,
                ThanhTien = (decimal)(ct.SoLuong ?? 0) * (ct.GiaBan ?? 0)
            }).ToList();

            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(details);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PhieuXuat_{id}.xlsx");
        }
    }
}

