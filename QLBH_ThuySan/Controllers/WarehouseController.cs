using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Warehouses (Kho)
    /// Maps to Kho and ChiTietTon tables in HieuHoaDB
    /// </summary>
    [Authorize]
    public class WarehouseController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: Warehouse - List all warehouses
        public async Task<IActionResult> Index()
        {
            var warehouses = await _context.Khos
                .Include(k => k.MaDaiLyPhuTrachNavigation)
                .Where(k => !k.IsDisabled)
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
        public async Task<IActionResult> Create()
        {
            ViewData["MaDaiLyPhuTrach"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.DaiLys, "MaDaiLy", "TenDaiLy");
            
            var model = new Kho
            {
                MaKho = await _codeGen.GenerateWarehouseCodeAsync()
            };
            return View(model);
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
                kho.IsDisabled = true;
                _context.Update(kho);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool KhoExists(string id)
        {
            return _context.Khos.Any(e => e.MaKho == id);
        }

        // GET: Warehouse/Inventory/KHO001 - View inventory for a specific warehouse
        public async Task<IActionResult> Inventory(string id, string searchMa, string searchTen, bool lowStock, string sortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Khos
                .Include(k => k.MaDaiLyPhuTrachNavigation)
                .FirstOrDefaultAsync(m => m.MaKho == id && !m.IsDisabled);

            if (warehouse == null)
            {
                return NotFound();
            }

            var query = _context.ChiTietTons
                .Include(ct => ct.MaHangNavigation)
                .Where(ct => ct.MaKho == id)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(searchMa))
                query = query.Where(ct => ct.MaHang.Contains(searchMa));
            
            if (!string.IsNullOrEmpty(searchTen))
                query = query.Where(ct => ct.MaHangNavigation != null && ct.MaHangNavigation.TenHang.Contains(searchTen));
            
            if (lowStock)
                query = query.Where(ct => (ct.SoLuongTon ?? 0) < 50);

            // Sorting
            ViewData["MaSort"] = string.IsNullOrEmpty(sortOrder) || sortOrder == "ma_desc" ? "ma_asc" : "ma_desc";
            ViewData["TenSort"] = sortOrder == "ten_asc" ? "ten_desc" : "ten_asc";
            ViewData["QtySort"] = sortOrder == "qty_asc" ? "qty_desc" : "qty_asc";
            ViewData["UnitSort"] = sortOrder == "unit_asc" ? "unit_desc" : "unit_asc";
            ViewData["ValueSort"] = sortOrder == "value_asc" ? "value_desc" : "value_asc";

            query = sortOrder switch
            {
                "ma_asc" => query.OrderBy(ct => ct.MaHang),
                "ma_desc" => query.OrderByDescending(ct => ct.MaHang),
                "ten_asc" => query.OrderBy(ct => ct.MaHangNavigation!.TenHang),
                "ten_desc" => query.OrderByDescending(ct => ct.MaHangNavigation!.TenHang),
                "qty_asc" => query.OrderBy(ct => ct.SoLuongTon),
                "qty_desc" => query.OrderByDescending(ct => ct.SoLuongTon),
                "unit_asc" => query.OrderBy(ct => ct.MaHangNavigation!.DonViTinh),
                "unit_desc" => query.OrderByDescending(ct => ct.MaHangNavigation!.DonViTinh),
                "value_asc" => query.OrderBy(ct => ct.GiaTriTon),
                "value_desc" => query.OrderByDescending(ct => ct.GiaTriTon),
                _ => query.OrderByDescending(ct => ct.SoLuongTon)
            };

            var inventory = await query.ToListAsync();

            ViewBag.Warehouse = warehouse;
            ViewBag.SearchMa = searchMa;
            ViewBag.SearchTen = searchTen;
            ViewBag.LowStock = lowStock;
            ViewBag.SortOrder = sortOrder;

            return View(inventory);
        }
    }
}
