using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Input Debt (Công Nợ Đầu Vào / Nhà Cung Cấp)
    /// Tracks Imports and Payables
    /// </summary>
    [Authorize]
    public class SupplierDebtController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierDebtController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SupplierDebt - List Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.NhaCungCaps.ToListAsync();
            // User requested fix: If no imports (or default), debt = 0
            foreach (var s in suppliers)
            {
                if (s.DuNoLuyKe == null) s.DuNoLuyKe = 0;
            }
            return View(suppliers);
        }

        // GET: SupplierDebt/Details/NCC001 - View Debt Details (Imports history)
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.NhaCungCaps
                .FirstOrDefaultAsync(m => m.MaDoiTuong == id);

            if (supplier == null)
            {
                return NotFound();
            }

            // Get Imports History
            var imports = await _context.PhieuNhaps
                .Where(p => p.IdNhaCungCap == id)
                .OrderByDescending(p => p.NgayNhap)
                .ToListAsync();
            
            ViewBag.Imports = imports;
            
            // Calculate Total Debt (Total Imports). 
            // Note: Does not subtract payments because PhieuThuChi is generic.
            var totalImport = imports.Sum(i => i.SoPhaiThanhToan ?? 0);
            ViewBag.TotalImportValue = totalImport;
            
            // User requested fix: Sync DuNoLuyKe with calculated value if needed, or default to 0
            if (supplier.DuNoLuyKe == null)
            {
                 supplier.DuNoLuyKe = totalImport;
            }

            return View(supplier);
        }
    }
}

