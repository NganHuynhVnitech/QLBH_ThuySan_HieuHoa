using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Warehouse
        public async Task<IActionResult> Index()
        {
            return View(await _context.Warehouses.ToListAsync());
        }

        // GET: Warehouse/Inventory/5
        public async Task<IActionResult> Inventory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(m => m.WarehouseId == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            var inventory = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .Where(pw => pw.WarehouseId == id)
                .ToListAsync();

            ViewBag.Warehouse = warehouse;
            return View(inventory);
        }
    }
}
