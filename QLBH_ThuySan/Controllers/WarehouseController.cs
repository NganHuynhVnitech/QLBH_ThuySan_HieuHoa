using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Warehouses (Kho)
    /// Maps to Kho and ChiTietTon tables in HieuHoaDB
    /// </summary>
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Warehouse - List all warehouses
        public async Task<IActionResult> Index()
        {
            var warehouses = await _context.Khos
                .Include(k => k.MaDaiLyPhuTrachNavigation)
                .ToListAsync();
            return View(warehouses);
        }

        // GET: Warehouse/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Khos
                .Include(k => k.MaDaiLyPhuTrachNavigation)
                .FirstOrDefaultAsync(m => m.MaKho == id);
            if (kho == null)
            {
                return NotFound();
            }

            return View(kho);
        }

        // GET: Warehouse/Create
        public IActionResult Create()
        {
            ViewData["MaDaiLyPhuTrach"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.DaiLys, "MaDaiLy", "TenDaiLy");
            return View();
        }

        // POST: Warehouse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKho,TenKho,LoaiKho,MaDaiLyPhuTrach")] Kho kho)
        {
            if (ModelState.IsValid)
            {
                if (_context.Khos.Any(e => e.MaKho == kho.MaKho))
                {
                    ModelState.AddModelError("MaKho", "Mã kho đã tồn tại.");
                    ViewData["MaDaiLyPhuTrach"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.DaiLys, "MaDaiLy", "TenDaiLy", kho.MaDaiLyPhuTrach);
                    return View(kho);
                }

                _context.Add(kho);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDaiLyPhuTrach"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.DaiLys, "MaDaiLy", "TenDaiLy", kho.MaDaiLyPhuTrach);
            return View(kho);
        }

        // GET: Warehouse/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Khos.FindAsync(id);
            if (kho == null)
            {
                return NotFound();
            }
            ViewData["MaDaiLyPhuTrach"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.DaiLys, "MaDaiLy", "TenDaiLy", kho.MaDaiLyPhuTrach);
            return View(kho);
        }

        // POST: Warehouse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKho,TenKho,LoaiKho,MaDaiLyPhuTrach")] Kho kho)
        {
            if (id != kho.MaKho)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kho);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoExists(kho.MaKho))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDaiLyPhuTrach"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.DaiLys, "MaDaiLy", "TenDaiLy", kho.MaDaiLyPhuTrach);
            return View(kho);
        }

        // POST: Warehouse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var kho = await _context.Khos.FindAsync(id);
            if (kho != null)
            {
                _context.Khos.Remove(kho);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool KhoExists(string id)
        {
            return _context.Khos.Any(e => e.MaKho == id);
        }

        // GET: Warehouse/Inventory/KHO001 - View inventory for a specific warehouse
        public async Task<IActionResult> Inventory(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Khos
                .Include(k => k.MaDaiLyPhuTrachNavigation)
                .FirstOrDefaultAsync(m => m.MaKho == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            var inventory = await _context.ChiTietTons
                .Include(ct => ct.MaHangNavigation)
                .Where(ct => ct.MaKho == id)
                .ToListAsync();

            ViewBag.Warehouse = warehouse;
            return View(inventory);
        }
    }
}
