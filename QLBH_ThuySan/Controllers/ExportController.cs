using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            return View(phieuXuat);
        }

        // GET: Export/Create
        public IActionResult Create()
        {
            ViewData["IdDaiLyBan"] = new SelectList(_context.DaiLys.Where(d => d.IsDisabled == false), "MaDaiLy", "TenDaiLy");
            return View();
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
            var details = new List<ChiTietPhieuXuat>();

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
                trangThaiThanhToan = p.TrangThaiThanhToan
            }).ToListAsync();

            return Json(result);
        }
    }
}

