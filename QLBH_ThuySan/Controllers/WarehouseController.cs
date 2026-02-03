using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Warehouses (Kho)
    /// Maps to Kho and ChiTietTon tables in HieuHoaDB
    /// </summary>
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
