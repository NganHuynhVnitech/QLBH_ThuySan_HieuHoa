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
        public async Task<IActionResult> Index(string type = "", DateTime? fromDate = null, DateTime? toDate = null)
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
            if (fromDate.HasValue)
            {
                query = query.Where(p => p.NgayXuat >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(p => p.NgayXuat <= toDate.Value.AddDays(1).AddTicks(-1));
            }

            var exports = await query.OrderByDescending(p => p.NgayXuat).ToListAsync();

            ViewData["CurrentType"] = type;
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
    }
}
