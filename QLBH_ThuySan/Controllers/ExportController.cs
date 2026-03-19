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
            ViewData["HangHoas"] = _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.GiaBanHienTai, h.DonViTinh }).ToList();
            ViewData["NhaCungCaps"] = _context.NhaCungCaps.Select(n => new { n.MaDoiTuong, n.TenDoiTuong }).ToList();

            return View();
        }
    }
}
