using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using System.Linq;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class DiscountCalculationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountCalculationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DiscountCalculation
        public async Task<IActionResult> Index(string? searchTerm, string? status, string? type, DateTime? fromDate, DateTime? toDate, string? ruleSearch, DateTime? periodFrom, DateTime? periodTo, DateTime? paymentFrom, DateTime? paymentTo)
        {
            var query = _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietPhieuTinhs)
                .AsQueryable();

            // 1. Basic Filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.TrangThai == status);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(p => p.LoaiDoiTuong == type);

            if (fromDate.HasValue)
                query = query.Where(p => p.NgayTao >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.NgayTao <= toDate.Value);

            if (periodFrom.HasValue)
                query = query.Where(p => p.TuNgay >= DateOnly.FromDateTime(periodFrom.Value));

            if (periodTo.HasValue)
                query = query.Where(p => p.DenNgay <= DateOnly.FromDateTime(periodTo.Value));

            if (paymentFrom.HasValue)
                query = query.Where(p => p.NgayThanhToan >= paymentFrom.Value);

            if (paymentTo.HasValue)
                query = query.Where(p => p.NgayThanhToan <= paymentTo.Value);
            
            if (!string.IsNullOrEmpty(ruleSearch))
                query = query.Where(p => p.ChiTietPhieuTinhs.Any(c => c.NoiDung != null && c.NoiDung.Contains(ruleSearch)));

            // Execute Query first (needed for in-memory name resolution)
            var list = await query.OrderByDescending(p => p.NgayTao).ToListAsync();

            // 2. Resolve Names & Advanced Search (Partner Name)
            // Collect IDs
            var nccIds = list.Where(p => p.LoaiDoiTuong == "NCC").Select(p => p.MaDoiTuong).Distinct().ToList();
            var khIds = list.Where(p => p.LoaiDoiTuong == "KHACH").Select(p => p.MaDoiTuong).Distinct().ToList();

            var nccNames = await _context.NhaCungCaps.Where(n => nccIds.Contains(n.MaDoiTuong)).ToDictionaryAsync(k => k.MaDoiTuong, v => v.TenDoiTuong ?? "");
            var khNames = await _context.KhachHangs.Where(k => khIds.Contains(k.MaDoiTuong)).ToDictionaryAsync(k => k.MaDoiTuong, v => v.TenDoiTuong ?? "");

            var viewModels = list.Select(p => {
                string name = p.MaDoiTuong ?? "";
                if (p.LoaiDoiTuong == "NCC" && nccNames.ContainsKey(p.MaDoiTuong!)) name = nccNames[p.MaDoiTuong!];
                else if (p.LoaiDoiTuong == "KHACH" && khNames.ContainsKey(p.MaDoiTuong!)) name = khNames[p.MaDoiTuong!];
                
                // Rule Summary: Get unique rules from details
                var rules = string.Join("; ", p.ChiTietPhieuTinhs.Select(c => c.NoiDung).Distinct());

                return new QLBH_ThuySan.Models.ViewModels.DiscountListItem
                {
                    MaPhieuTinh = p.MaPhieuTinh,
                    NgayTao = p.NgayTao ?? DateTime.MinValue,
                    LoaiDoiTuong = p.LoaiDoiTuong ?? "",
                    MaDoiTuong = p.MaDoiTuong ?? "",
                    TenDoiTuong = name ?? "",
                    TuNgay = p.TuNgay,
                    DenNgay = p.DenNgay,
                    SoPhaiThanhToan = p.SoPhaiThanhToan ?? 0,
                    SoDaThanhToan = p.SoDaThanhToan ?? 0,
                    SoChuaThanhToan = p.SoChuaThanhToan ?? 0,
                    TrangThai = p.TrangThai ?? "",
                    NgayThanhToan = p.NgayThanhToan,
                    QuyTacChietKhau = rules.Length > 50 ? rules.Substring(0, 50) + "..." : rules
                };
            }).ToList();

            // 3. Filter by Partner Name/Code (In Memory)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                viewModels = viewModels.Where(v => v.TenDoiTuong.ToLower().Contains(searchTerm) || v.MaDoiTuong.ToLower().Contains(searchTerm)).ToList();
            }

            var vm = new QLBH_ThuySan.Models.ViewModels.DiscountListViewModel
            {
                SearchTerm = searchTerm,
                Status = status,
                Type = type,
                FromDate = fromDate,
                ToDate = toDate,
                PeriodFrom = periodFrom,
                PeriodTo = periodTo,
                PaymentFrom = paymentFrom,
                PaymentTo = paymentTo,
                RuleSearch = ruleSearch,
                Items = viewModels
            };

            return View(vm);
        }

        // GET: DiscountCalculation/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietPhieuTinhs)
                .ThenInclude(c => c.MaHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhieuTinh == id);

            if (phieu == null) return NotFound();

             // Get Partner Name
            string partnerName = phieu.MaDoiTuong!;
            if (phieu.LoaiDoiTuong == "NCC")
            {
                var ncc = await _context.NhaCungCaps.FindAsync(phieu.MaDoiTuong);
                if (ncc != null) partnerName = ncc.TenDoiTuong ?? phieu.MaDoiTuong!;
            }
            else
            {
                var kh = await _context.KhachHangs.FindAsync(phieu.MaDoiTuong);
                if (kh != null) partnerName = kh.TenDoiTuong ?? phieu.MaDoiTuong!;
            }
            ViewBag.PartnerName = partnerName;

            return View(phieu);
        }

        // GET: DiscountCalculation/Create
        public IActionResult Create()
        {
            return View();
        }

        // API: Search Products
        [HttpGet]
        public async Task<IActionResult> SearchProduct(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());

            var products = await _context.HangHoas
                .Where(h => h.TenHang.Contains(term) || h.MaHang.Contains(term))
                .Take(20)
                .Select(h => new { h.MaHang, h.TenHang, h.DonViTinh })
                .ToListAsync();
            return Json(products);
        }

        // API: Search Transactions for SPECIFIC Product
        [HttpGet]
        public async Task<IActionResult> GetProductTransactions(string type, string partnerId, string productId, string fromDate, string toDate)
        {
            if (string.IsNullOrEmpty(partnerId) || string.IsNullOrEmpty(productId))
                return BadRequest("Thiếu thông tin tra cứu");

            DateTime start = DateTime.Parse(fromDate);
            DateTime end = DateTime.Parse(toDate);
            
            // Return List of receipts for this product
            // Model: Date, PhieuID, Qty
            
            if (type == "NCC")
            {
                var imports = await _context.ChiTietPhieuNhaps
                    .Include(ct => ct.MaPhieuNavigation)
                    .Include(ct => ct.MaHangNavigation)
                    .Where(ct => ct.MaPhieuNavigation.IdNhaCungCap == partnerId 
                              && ct.MaHang == productId
                              && ct.MaPhieuNavigation.NgayNhap >= start 
                              && ct.MaPhieuNavigation.NgayNhap <= end)
                    .OrderBy(ct => ct.MaPhieuNavigation.NgayNhap)
                    .Select(ct => new 
                    {
                        ngay = ct.MaPhieuNavigation.NgayNhap,
                        maPhieu = ct.MaPhieu,
                        tenHang = ct.MaHangNavigation.TenHang,
                        soLuong = ct.SoLuong ?? 0
                    })
                    .ToListAsync();
                return Json(imports);
            }
            else // KHACH
            {
                var exports = await _context.ChiTietPhieuXuats
                    .Include(ct => ct.MaPhieuNavigation)
                    .Include(ct => ct.MaHangNavigation)
                    .Where(ct => ct.MaPhieuNavigation.IdKhachHang == partnerId 
                              && ct.MaHang == productId
                              && ct.MaPhieuNavigation.NgayXuat >= start 
                              && ct.MaPhieuNavigation.NgayXuat <= end)
                    .OrderBy(ct => ct.MaPhieuNavigation.NgayXuat)
                    .Select(ct => new 
                    {
                        ngay = ct.MaPhieuNavigation.NgayXuat,
                        maPhieu = ct.MaPhieu,
                        tenHang = ct.MaHangNavigation.TenHang,
                        soLuong = ct.SoLuong ?? 0
                    })
                    .ToListAsync();
                return Json(exports);
            }
        }

        // POST: DiscountCalculation/Create
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DiscountSaveRequest request)
        {
            if (request == null || !request.Items.Any()) return BadRequest("Dữ liệu không hợp lệ");

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. Create Header
                var phieu = new PhieuTinhChietKhau
                {
                    MaPhieuTinh = "CK" + DateTime.Now.ToString("yyMMddHHmm"),
                    LoaiDoiTuong = request.Type,
                    MaDoiTuong = request.PartnerId,
                    TuNgay = DateOnly.FromDateTime(DateTime.Parse(request.FromDate)),
                    DenNgay = DateOnly.FromDateTime(DateTime.Parse(request.ToDate)),
                    NgayTao = DateTime.Now,
                    TrangThai = "Chưa thanh toán",
                    SoPhaiThanhToan = 0 // Will sum below
                };
                
                _context.PhieuTinhChietKhaus.Add(phieu);
                await _context.SaveChangesAsync(); // Save to get valid state

                // 2. Create Details
                decimal totalAmount = 0;
                foreach (var item in request.Items)
                {
                    if (item.IsSelected)
                    {
                        decimal discountAmt = 0;
                        string note = "";

                        // Rule Logic
                        if (item.RuleType == "Fixed")
                        {
                            discountAmt = (decimal)item.TotalQty * item.FixedRate;
                            note = $"Cố định: {item.FixedRate:N0}/đơn vị";
                        }
                        else if (item.RuleType == "Tiered")
                        {
                            // Parse Tier ranges (Simple example: Tier 1 > 0, Rate 1)
                            // In real app, this would interpret the complex Tier string.
                            // For Demo: simplified logic or trust the frontend calculated amount if complex
                            // Here we trust the calculated math mostly, but re-verify basic math.
                            // Let's assume frontend sends the calculated Amount for flexibility in this demo.
                            discountAmt = item.CalculatedAmount; 
                            note = item.RuleDescription;
                        }

                        var detail = new ChiTietPhieuTinh
                        {
                            MaPhieuTinh = phieu.MaPhieuTinh,
                            MaHang = item.MaHang,
                            SoLuong = item.TotalQty,
                            SoTienChietKhau = item.FixedRate > 0 ? item.FixedRate : 0, // Rate or 0 if tiered
                            ThanhTien = discountAmt,
                            NoiDung = note
                        };
                        _context.ChiTietPhieuTinhs.Add(detail);
                        totalAmount += discountAmt;
                    }
                }

                // 3. Update Header Total
                phieu.SoPhaiThanhToan = totalAmount;
                phieu.SoDaThanhToan = 0;
                phieu.SoChuaThanhToan = totalAmount;
                _context.Update(phieu);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, id = phieu.MaPhieuTinh });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Lỗi khi lưu: " + ex.Message);
            }
        }

        // GET: DiscountCalculation/Print/5
        public async Task<IActionResult> Print(string? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietPhieuTinhs)
                .ThenInclude(c => c.MaHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhieuTinh == id);

            if (phieu == null) return NotFound();

            // Resolve Partner Name
            string partnerName = phieu.MaDoiTuong!;
            if (phieu.LoaiDoiTuong == "NCC")
            {
                var ncc = await _context.NhaCungCaps.FindAsync(phieu.MaDoiTuong);
                if (ncc != null) partnerName = ncc.TenDoiTuong ?? phieu.MaDoiTuong!;
            }
            else
            {
                var kh = await _context.KhachHangs.FindAsync(phieu.MaDoiTuong);
                if (kh != null) partnerName = kh.TenDoiTuong ?? phieu.MaDoiTuong!;
            }

            var vm = new QLBH_ThuySan.Models.ViewModels.DiscountPrintViewModel
            {
                MaPhieuTinh = phieu.MaPhieuTinh,
                NgayTao = phieu.NgayTao ?? DateTime.Now,
                LoaiDoiTuong = phieu.LoaiDoiTuong!,
                MaDoiTuong = phieu.MaDoiTuong!,
                TenDoiTuong = partnerName,
                TuNgay = phieu.TuNgay?.ToDateTime(TimeOnly.MinValue),
                DenNgay = phieu.DenNgay?.ToDateTime(TimeOnly.MinValue),
                SoPhaiThanhToanChietKhau = phieu.SoPhaiThanhToan ?? 0,
                TrangThai = phieu.TrangThai ?? "",
                NgayThanhToan = phieu.NgayThanhToan
            };

            // Fetch Details for each Product
            foreach (var item in phieu.ChiTietPhieuTinhs)
            {
                var productItem = new QLBH_ThuySan.Models.ViewModels.DiscountPrintProductItem
                {
                    TenHang = item.MaHangNavigation?.TenHang ?? item.MaHang ?? "",
                    DonViTinh = item.MaHangNavigation?.DonViTinh ?? "",
                    TongSoLuong = item.SoLuong ?? 0,
                    SoPhaiThanhToanChietKhau = item.ThanhTien ?? 0,
                    QuyTacApDung = item.NoiDung ?? ""
                };

                // Query Logic (Similar to GetProductTransactions)
                DateTime start = phieu.TuNgay?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
                DateTime end = phieu.DenNgay?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.MaxValue.Date.AddDays(1).AddTicks(-1); // End of day

                List<QLBH_ThuySan.Models.ViewModels.DiscountTransactionDetail> transDetails = new();

                if (phieu.LoaiDoiTuong == "NCC")
                {
                    var imports = await _context.ChiTietPhieuNhaps
                        .Include(ct => ct.MaPhieuNavigation)
                        .Where(ct => ct.MaPhieuNavigation.IdNhaCungCap == phieu.MaDoiTuong 
                                  && ct.MaHang == item.MaHang
                                  && ct.MaPhieuNavigation.NgayNhap >= start 
                                  && ct.MaPhieuNavigation.NgayNhap <= end)
                        .OrderBy(ct => ct.MaPhieuNavigation.NgayNhap)
                        .ToListAsync();

                    foreach(var imp in imports)
                    {
                        var t = new QLBH_ThuySan.Models.ViewModels.DiscountTransactionDetail
                        {
                            NgayGiaoDich = imp.MaPhieuNavigation.NgayNhap ?? DateTime.MinValue,
                            MaPhieu = imp.MaPhieu,
                            SoLuong = imp.SoLuong ?? 0,
                            DonGiaMua = imp.DonGiaNhap ?? 0,
                            ThanhTienMua = (decimal)(imp.SoLuong ?? 0) * (imp.DonGiaNhap ?? 0)
                        };
                        transDetails.Add(t);
                    }
                }
                else // KHACH
                {
                    var exports = await _context.ChiTietPhieuXuats
                        .Include(ct => ct.MaPhieuNavigation)
                        .Where(ct => ct.MaPhieuNavigation.IdKhachHang == phieu.MaDoiTuong 
                                  && ct.MaHang == item.MaHang
                                  && ct.MaPhieuNavigation.NgayXuat >= start 
                                  && ct.MaPhieuNavigation.NgayXuat <= end)
                        .OrderBy(ct => ct.MaPhieuNavigation.NgayXuat)
                        .ToListAsync();

                    foreach(var exp in exports)
                    {
                        var t = new QLBH_ThuySan.Models.ViewModels.DiscountTransactionDetail
                        {
                            NgayGiaoDich = exp.MaPhieuNavigation.NgayXuat ?? DateTime.MinValue,
                            MaPhieu = exp.MaPhieu,
                            SoLuong = exp.SoLuong ?? 0,
                            DonGiaMua = exp.GiaBan ?? 0,
                            ThanhTienMua = (decimal)(exp.SoLuong ?? 0) * (exp.GiaBan ?? 0)
                        };
                        transDetails.Add(t);
                    }
                }

                // Distribute Discount (Pro-rata or Fixed logic)
                // If Item.SoTienChietKhau > 0 (Fixed Rate stored), use it.
                // Else calculate average rate.
                decimal appliedRate = 0;
                if (item.SoTienChietKhau > 0) 
                {
                    appliedRate = item.SoTienChietKhau.Value;
                }
                else if (item.SoLuong > 0)
                {
                    appliedRate = (item.ThanhTien ?? 0) / (decimal)item.SoLuong;
                }

                foreach(var t in transDetails)
                {
                    t.DonGiaChietKhau = appliedRate;
                    t.ThanhTienChietKhau = (decimal)t.SoLuong * appliedRate;
                }

                productItem.Transactions = transDetails;
                vm.ProductDetails.Add(productItem);
            }

            return View(vm);
        }

    public class TransactionItemViewModel
    {
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string? DonViTinh { get; set; }
        public double TongSoLuong { get; set; }
    }

    public class DiscountSaveRequest 
    {
        public string Type { get; set; } = ""; // NCC / KHACH
        public string PartnerId { get; set; } = "";
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public List<DiscountSaveItem> Items { get; set; } = new();
    }

    public class DiscountSaveItem
    {
        public bool IsSelected { get; set; }
        public string MaHang { get; set; } = "";
        public double TotalQty { get; set; }
        public string RuleType { get; set; } = "Fixed"; // Fixed / Tiered
        public decimal FixedRate { get; set; }
        public string TierConfig { get; set; } = ""; // JSON or String
        public decimal CalculatedAmount { get; set; }
        public string RuleDescription { get; set; } = "";
    }
    }
}


