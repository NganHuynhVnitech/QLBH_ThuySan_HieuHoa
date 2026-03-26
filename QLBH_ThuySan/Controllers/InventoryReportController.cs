using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using QLBH_ThuySan.Models;
using System.Data;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class InventoryReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["MaKho"] = new SelectList(await _context.Khos.Where(k => !k.IsDisabled).ToListAsync(), "MaKho", "TenKho");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetReportData(int month, int year, string? maKho)
        {
            try
            {
                var data = await CalculateInventoryReport(month, year, maKho);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<InventoryReportItem>> CalculateInventoryReport(int month, int year, string? maKho)
        {
            // 1. Check if snapshot exists
            var snapshots = await _context.TonKhoChotKys
                .Include(s => s.MaHangNavigation)
                .Include(s => s.MaKhoNavigation)
                .Where(s => s.Thang == month && s.Nam == year && (string.IsNullOrEmpty(maKho) || s.MaKho == maKho))
                .ToListAsync();

            if (snapshots.Any())
            {
                return snapshots.Select(s => new InventoryReportItem
                {
                    MaHang = s.MaHang,
                    TenHang = s.MaHangNavigation?.TenHang ?? "N/A",
                    DonViTinh = s.MaHangNavigation?.DonViTinh ?? "",
                    TonDau = s.SoLuongTonDau ?? 0,
                    TienTonDau = s.SoLuongTonDau * (double?)(s.MaHangNavigation?.GiaVonHienTai ?? 0) ?? 0, // Fallback to current if missing in snapshot? 
                    // Actually, let's use the snapshot's value for the end of the previous period.
                    Nhap = s.SoLuongNhap ?? 0,
                    TienNhap = s.SoLuongNhap * (double?)(s.MaHangNavigation?.GiaVonHienTai ?? 0) ?? 0, // These snapshots should probably have historical values.
                    Xuat = s.SoLuongXuat ?? 0,
                    TienXuat = s.SoLuongXuat * (double?)(s.MaHangNavigation?.GiaVonHienTai ?? 0) ?? 0,
                    TonCuoi = s.SoLuongTonCuoi ?? 0,
                    GiaTriCuoi = s.GiaTriTonCuoi ?? 0,
                    IsClosed = true,
                    MaKho = s.MaKho,
                    TenKho = s.MaKhoNavigation?.TenKho ?? s.MaKho
                }).ToList();
            }

            // 2. Calculate dynamically
            var prevMonth = month == 1 ? 12 : month - 1;
            var prevYear = month == 1 ? year - 1 : year;

            var prevSnapshots = await _context.TonKhoChotKys
                .Where(s => s.Thang == prevMonth && s.Nam == prevYear)
                .ToDictionaryAsync(s => s.MaKho + "_" + s.MaHang);

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            // Get all products and warehouses
            var products = await _context.HangHoas.ToListAsync();
            var warehouses = await _context.Khos.Where(k => string.IsNullOrEmpty(maKho) || k.MaKho == maKho).ToListAsync();

            // Get all transactions in the period
            var imports = await _context.ChiTietPhieuNhaps
                .Include(ct => ct.MaPhieuNavigation)
                .Where(ct => ct.MaPhieuNavigation.NgayNhap >= startDate && ct.MaPhieuNavigation.NgayNhap <= endDate && !ct.MaPhieuNavigation.IsDisabled)
                .ToListAsync();

            var exports = await _context.ChiTietPhieuXuats
                .Include(ct => ct.MaPhieuNavigation)
                .Where(ct => ct.MaPhieuNavigation.NgayXuat >= startDate && ct.MaPhieuNavigation.NgayXuat <= endDate && !ct.MaPhieuNavigation.IsDisabled)
                .ToListAsync();

            var result = new List<InventoryReportItem>();

            foreach (var w in warehouses)
            {
                // Note: For imports, we map via delegate's first physical warehouse as per system logic
                foreach (var h in products)
                {
                    var key = w.MaKho + "_" + h.MaHang;
                    double tonDau = prevSnapshots.TryGetValue(key, out var snap) ? (snap.SoLuongTonCuoi ?? 0) : 0;
                    double tienTonDau = (double)(prevSnapshots.TryGetValue(key, out var snapTien) ? (snapTien.GiaTriTonCuoi ?? 0) : 0);

                    // Calculate Nhap
                    var itemImports = imports.Where(i => i.MaHang == h.MaHang && 
                                   _context.Khos.FirstOrDefault(k => k.MaDaiLyPhuTrach == i.MaPhieuNavigation.IdDaiLyNhap && k.LoaiKho == "VAT_LY")?.MaKho == w.MaKho);
                    double nhap = itemImports.Sum(i => i.SoLuong ?? 0);
                    decimal tienNhap = itemImports.Sum(i => (decimal)(i.SoLuong ?? 0) * (i.DonGiaNhap ?? 0));

                    // Calculate Xuat
                    double xuat = exports
                        .Where(e => e.MaHang == h.MaHang && (e.MaPhieuNavigation.MaKhoXuat == w.MaKho))
                        .Sum(e => e.SoLuong ?? 0);
                    decimal tienXuat = (decimal)xuat * (h.GiaVonHienTai ?? 0);

                    double tonCuoi = tonDau + nhap - xuat;
                    decimal giaTriCuoi = (decimal)tonCuoi * (h.GiaVonHienTai ?? 0);

                    if (tonDau != 0 || nhap != 0 || xuat != 0 || tonCuoi != 0)
                    {
                        result.Add(new InventoryReportItem
                        {
                            MaHang = h.MaHang,
                            TenHang = h.TenHang ?? "N/A",
                            DonViTinh = h.DonViTinh ?? "",
                            TonDau = tonDau,
                            TienTonDau = tienTonDau,
                            Nhap = nhap,
                            TienNhap = (double)tienNhap,
                            Xuat = xuat,
                            TienXuat = (double)tienXuat,
                            TonCuoi = tonCuoi,
                            GiaTriCuoi = giaTriCuoi,
                            IsClosed = false,
                            MaKho = w.MaKho,
                            TenKho = w.TenKho
                        });
                    }
                }
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CloseInventory(int month, int year)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Check if already closed
                if (await _context.TonKhoChotKys.AnyAsync(s => s.Thang == month && s.Nam == year))
                {
                    return BadRequest("Tháng này đã được chốt tồn kho.");
                }

                var reportData = await CalculateInventoryReport(month, year, null);
                
                foreach (var item in reportData)
                {
                    var snapshot = new TonKhoChotKy
                    {
                        Thang = month,
                        Nam = year,
                        MaKho = item.MaKho ?? "",
                        MaHang = item.MaHang,
                        SoLuongTonDau = item.TonDau,
                        SoLuongNhap = item.Nhap,
                        SoLuongXuat = item.Xuat,
                        SoLuongTonCuoi = item.TonCuoi,
                        GiaTriTonCuoi = item.GiaTriCuoi
                    };
                    _context.TonKhoChotKys.Add(snapshot);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Chốt tồn kho thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Lỗi khi chốt tồn kho: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(int month, int year, string? maKho)
        {
            var data = await CalculateInventoryReport(month, year, maKho);
            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(data);
            memoryStream.Seek(0, SeekOrigin.Begin);
            
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoXNT_{month}_{year}.xlsx");
        }

        public class InventoryReportItem
        {
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DonViTinh { get; set; } = "";
            public double TonDau { get; set; }
            public double TienTonDau { get; set; }
            public double Nhap { get; set; }
            public double TienNhap { get; set; }
            public double Xuat { get; set; }
            public double TienXuat { get; set; }
            public double TonCuoi { get; set; }
            public decimal GiaTriCuoi { get; set; }
            public bool IsClosed { get; set; }
            public string? MaKho { get; set; }
            public string? TenKho { get; set; }
        }
    }
}
