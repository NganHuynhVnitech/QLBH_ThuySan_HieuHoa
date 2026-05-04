using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;
using System.IO;
using System.Linq;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class DiscountCalculationController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: DiscountCalculation
        public async Task<IActionResult> Index(string? searchTerm, string? status, string? type, DateTime? fromDate, DateTime? toDate, string? ruleSearch, DateTime? periodFrom, DateTime? periodTo, DateTime? paymentFrom, DateTime? paymentTo, string? sortColumn, string? sortOrder)
        {
            var query = _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietChietKhaus)
                .Where(p => !p.IsDisabled)
                .AsQueryable();

            // 1. Basic Filters
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Chưa Thanh Toán")
                {
                    query = query.Where(p => p.TrangThai == "Chưa Thanh Toán" || p.TrangThai == "Thanh Toán Một Phần");
                }
                else
                {
                    query = query.Where(p => p.TrangThai == status);
                }
            }


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
                query = query.Where(p => p.ChiTietChietKhaus.Any(c => c.NoiDung != null && c.NoiDung.Contains(ruleSearch)));

            // 3. Sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                bool isAsc = (sortOrder == "asc");
                switch (sortColumn)
                {
                    case "MaPhieu":
                        query = isAsc ? query.OrderBy(p => p.MaPhieuTinh) : query.OrderByDescending(p => p.MaPhieuTinh);
                        break;
                    case "TenPhieu":
                        query = isAsc ? query.OrderBy(p => p.TenPhieu) : query.OrderByDescending(p => p.TenPhieu);
                        break;
                    case "NgayTao":
                        query = isAsc ? query.OrderBy(p => p.NgayTao) : query.OrderByDescending(p => p.NgayTao);
                        break;
                    case "Loai":
                        query = isAsc ? query.OrderBy(p => p.LoaiDoiTuong) : query.OrderByDescending(p => p.LoaiDoiTuong);
                        break;
                    case "DoiTuong":
                        query = isAsc ? query.OrderBy(p => p.MaDoiTuong) : query.OrderByDescending(p => p.MaDoiTuong);
                        break;
                    case "NgayThanhToan":
                        query = isAsc ? query.OrderBy(p => p.NgayThanhToan) : query.OrderByDescending(p => p.NgayThanhToan);
                        break;
                    case "PhaiTT":
                        query = isAsc ? query.OrderBy(p => p.SoPhaiThanhToan) : query.OrderByDescending(p => p.SoPhaiThanhToan);
                        break;
                    case "DaTT":
                        query = isAsc ? query.OrderBy(p => p.SoDaThanhToan) : query.OrderByDescending(p => p.SoDaThanhToan);
                        break;
                    case "ConNo":
                        query = isAsc ? query.OrderBy(p => p.SoChuaThanhToan) : query.OrderByDescending(p => p.SoChuaThanhToan);
                        break;
                    case "TrangThai":
                        query = isAsc ? query.OrderBy(p => p.TrangThai) : query.OrderByDescending(p => p.TrangThai);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.NgayTao);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.NgayTao);
            }

            // Execute Query
            var list = await query.ToListAsync();

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
                var rules = string.Join("; ", p.ChiTietChietKhaus.Select(c => c.NoiDung).Distinct());

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
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Items = viewModels
            };

            return View(vm);
        }

        // GET: DiscountCalculation/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietChietKhaus)
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
        public async Task<IActionResult> Create()
        {
            var model = new PhieuTinhChietKhau
            {
                MaPhieuTinh = await _codeGen.GenerateDiscountCodeAsync(),
                NgayTao = DateTime.Now,
                TrangThai = "Chưa Thanh Toán"
            };
            return View(model);
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
            if (request == null || request.Items.Count == 0) return BadRequest("Dữ liệu không hợp lệ");

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                string maPhieuTinh = "CK" + DateTime.Now.ToString("yyMMddHHmm");
                decimal totalAmount = request.Items.Sum(i => i.CalculatedAmount);

                // 1. Insert Header via SP
                // SP Params: @maPhieuTinh, @tenPhieu, @loaiDoiTuong, @maDoiTuong, @tuNgay, @denNgay, @soPhaiThanhToan, @trangThai
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_PhieuTinhChietKhau_Insert {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                    maPhieuTinh, 
                    request.TenPhieu ?? "Phiếu chiết khấu", 
                    request.Type, 
                    request.PartnerId, 
                    DateTime.Parse(request.FromDate), 
                    DateTime.Parse(request.ToDate), 
                    totalAmount, 
                    "Chưa Thanh Toán"
                );

                // 2. Insert Details via SP
                foreach (var item in request.Items)
                {
                    decimal giaCK = item.FixedRate > 0 ? item.FixedRate : (item.TotalQty > 0 ? Math.Round(item.CalculatedAmount / (decimal)item.TotalQty, 2) : 0);
                    // SP Params: @maPhieuTinh, @maHang, @soLuong, @giaChietKhau, @thanhTien, @noiDung
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_ChiTietChietKhau_Insert {0}, {1}, {2}, {3}, {4}, {5}",
                        maPhieuTinh, item.MaHang, item.TotalQty, giaCK, item.CalculatedAmount, item.RuleDescription ?? ""
                    );
                }

                // 3. Handle Immediate Payment
                if (request.PayNow)
                {
                    // Reload the newly created slip to ensure EF tracker has it properly
                    var phieu = await _context.PhieuTinhChietKhaus.FirstOrDefaultAsync(p => p.MaPhieuTinh == maPhieuTinh);
                    if (phieu != null) 
                    {
                        await PerformPaymentInternal(phieu);
                    }
                }

                await transaction.CommitAsync();
                return Json(new { success = true, id = maPhieuTinh });
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
                .Include(p => p.ChiTietChietKhaus)
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
            foreach (var item in phieu.ChiTietChietKhaus)
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

                List<QLBH_ThuySan.Models.ViewModels.DiscountTransactionDetail> transDetails = [];

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
                // If Item.GiaChietKhau > 0 (Fixed Rate stored), use it.
                // Else calculate average rate.
                decimal appliedRate = 0;
                if (item.GiaChietKhau > 0) 
                {
                    appliedRate = item.GiaChietKhau.Value;
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
            public string TenPhieu { get; set; } = "";
            public string Type { get; set; } = ""; // NCC / KHACH
            public string PartnerId { get; set; } = "";
            public string FromDate { get; set; } = "";
            public string ToDate { get; set; } = "";
            public bool PayNow { get; set; }
            public List<DiscountSaveItem> Items { get; set; } = [];
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

        [HttpPost]
        public async Task<IActionResult> Pay(string id, decimal amount, string? dienGiai)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Mã phiếu không hợp lệ");

            var phieu = await _context.PhieuTinhChietKhaus.FirstOrDefaultAsync(p => p.MaPhieuTinh == id);
            if (phieu == null) return NotFound("Không tìm thấy phiếu chiết khấu");
            if (phieu.TrangThai == "Đã thanh toán") return BadRequest("Phiếu này đã được thanh toán");

            if (amount <= 0) return BadRequest("Số tiền không hợp lệ");

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await PerformPaymentInternal(phieu, amount, dienGiai);
                await transaction.CommitAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Lỗi khi thanh toán: " + ex.Message);
            }
        }

        private async Task PerformPaymentInternal(PhieuTinhChietKhau phieu, decimal? customAmount = null, string? customDienGiai = null)
        {
            decimal amount = customAmount ?? (phieu.SoChuaThanhToan ?? phieu.SoPhaiThanhToan ?? 0);
            if (amount <= 0)
            {
                if ((phieu.SoChuaThanhToan ?? 0) <= 0)
                {
                    phieu.TrangThai = "Đã thanh toán";
                    phieu.NgayThanhToan = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // 1. Create PhieuThuChi
            string loaiPhieu = phieu.LoaiDoiTuong == "NCC" ? "THU CHIET KHAU NCC" : "CHI CHIET KHAU KHACH HANG";
            string prefix = loaiPhieu.StartsWith("THU") ? "PT" : "PC";
            string maPhieuTC = prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString();
            if (maPhieuTC.Length > 20) maPhieuTC = maPhieuTC.Substring(0, 20);

            string description = customDienGiai ?? (phieu.LoaiDoiTuong == "NCC"
                ? $"Thu tiền chiết khấu từ NCC cho phiếu {phieu.MaPhieuTinh}"
                : $"Chi trả chiết khấu cho khách hàng cho phiếu {phieu.MaPhieuTinh}");

            var ptc = new PhieuThuChi
            {
                MaPhieu = maPhieuTC,
                LoaiPhieu = loaiPhieu,
                NgayLap = DateTime.Now,
                SoTien = amount,
                LyDo = description,
                MaDoiTuong = phieu.MaDoiTuong,
                LoaiDoiTuong = phieu.LoaiDoiTuong
            };
            _context.PhieuThuChis.Add(ptc);

            // 2. Update Ledger (SoRieng) and Debt
            if (phieu.LoaiDoiTuong == "NCC")
            {
                var sr = new SoRiengNhaCungCap
                {
                    MaNhaCungCap = phieu.MaDoiTuong,
                    NgayGiaoDich = DateTime.Now,
                    LoaiGiaoDich = "THU_CK",
                    SoTienPhatSinh = amount,
                    DienGiai = description.Length > 200 ? description.Substring(0, 200) : description
                };
                _context.SoRiengNhaCungCaps.Add(sr);

                var ncc = await _context.NhaCungCaps.FindAsync(phieu.MaDoiTuong);
                if (ncc != null)
                {
                    ncc.DuNoLuyKe = (ncc.DuNoLuyKe ?? 0) - amount;
                    _context.Update(ncc);
                }
            }
            else
            {
                var sr = new SoRiengKhachHang
                {
                    MaKhachHang = phieu.MaDoiTuong,
                    NgayGiaoDich = DateTime.Now,
                    LoaiGiaoDich = "CHI_CK",
                    SoTienPhatSinh = amount,
                    DienGiai = description.Length > 200 ? description.Substring(0, 200) : description
                };
                _context.SoRiengKhachHangs.Add(sr);

                var kh = await _context.KhachHangs.FindAsync(phieu.MaDoiTuong);
                if (kh != null)
                {
                    kh.DuNoLuyKe = (kh.DuNoLuyKe ?? 0) - amount;
                    _context.Update(kh);
                }
            }

            // 3. Update Discount Slip
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

            await _context.SaveChangesAsync();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phieu = await _context.PhieuTinhChietKhaus.FindAsync(id);
            if (phieu != null)
            {
                phieu.IsDisabled = true;
                _context.Update(phieu);
                
                // Find and disable associated PhieuThuChi (vouchers)
                var associatedVouchers = await _context.PhieuThuChis
                    .Where(v => v.LyDo != null && v.LyDo.Contains(phieu.MaPhieuTinh) && !v.IsDisabled)
                    .ToListAsync();
                
                foreach (var v in associatedVouchers)
                {
                    v.IsDisabled = true;
                    _context.Update(v);
                    
                    // Reverse DuNoLuyKe for these vouchers
                    decimal amount = v.SoTien ?? 0;
                    if (v.LoaiPhieu != null && (v.LoaiDoiTuong == "KH" || v.LoaiPhieu.Contains("KHACH HANG")) && !string.IsNullOrEmpty(v.MaDoiTuong))
                    {
                        var kh = await _context.KhachHangs.FindAsync(v.MaDoiTuong);
                        if (kh != null)
                        {
                            if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("THU")) kh.DuNoLuyKe += amount;
                            else kh.DuNoLuyKe -= amount;
                        }
                    }
                    else if (v.LoaiPhieu != null && (v.LoaiDoiTuong == "NCC" || v.LoaiPhieu.Contains("NCC")) && !string.IsNullOrEmpty(v.MaDoiTuong))
                    {
                        var ncc = await _context.NhaCungCaps.FindAsync(v.MaDoiTuong);
                        if (ncc != null)
                        {
                            if (v.LoaiPhieu == "THU CHIET KHAU NCC") ncc.DuNoLuyKe += amount;
                            else if (v.LoaiPhieu != null && v.LoaiPhieu.StartsWith("CHI")) ncc.DuNoLuyKe += amount;
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
            var phieu = await _context.PhieuTinhChietKhaus
                .Include(p => p.ChiTietChietKhaus)
                .ThenInclude(c => c.MaHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhieuTinh == id && !m.IsDisabled);

            if (phieu == null) return NotFound();

            var details = phieu.ChiTietChietKhaus.Select((ct, index) => new {
                STT = index + 1,
                MaHang = ct.MaHang,
                TenHang = ct.MaHangNavigation?.TenHang,
                DVT = ct.MaHangNavigation?.DonViTinh,
                SoLuong = ct.SoLuong,
                GiaChietKhau = ct.GiaChietKhau,
                ThanhTien = ct.ThanhTien,
                NoiDung = ct.NoiDung
            }).ToList();

            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(details);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ChietKhau_{id}.xlsx");
        }
    }
}
