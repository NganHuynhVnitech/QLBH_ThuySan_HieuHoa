using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            return View(await _context.HangHoas.ToListAsync());
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas
                .Include(h => h.DonViTinhs)
                .FirstOrDefaultAsync(m => m.MaHang == id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaHang,TenHang,DonViTinh,QuyCach,GiaVonHienTai,GiaBanHienTai")] HangHoa hangHoa, List<DonViTinh> units)
        {
            if (ModelState.IsValid)
            {
                if (_context.HangHoas.Any(e => e.MaHang == hangHoa.MaHang))
                {
                    ModelState.AddModelError("MaHang", "Mã hàng đã tồn tại.");
                    return View(hangHoa);
                }

                _context.Add(hangHoa);
                await _context.SaveChangesAsync(); // Save header first

                // Save Units
                if (units != null && units.Count > 0)
                {
                    foreach (var u in units)
                    {
                        if (!string.IsNullOrEmpty(u.TenDonVi))
                        {
                            u.MaHang = hangHoa.MaHang;
                            _context.Add(u);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(hangHoa);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas
                .Include(h => h.DonViTinhs)
                .FirstOrDefaultAsync(h => h.MaHang == id);
            if (hangHoa == null)
            {
                return NotFound();
            }
            return View(hangHoa);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaHang,TenHang,DonViTinh,QuyCach,GiaVonHienTai,GiaBanHienTai")] HangHoa hangHoa, List<DonViTinh> units)
        {
            if (id != hangHoa.MaHang)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hangHoa);
                    
                    // Sync Units: Remove old, Add new (Simple approach)
                    // Or smarter: Update existing, Add new, Delete missing.
                    // For simplicity: Load existing, compare.
                    
                    var existingUnits = _context.DonViTinhs.Where(u => u.MaHang == id).ToList();
                    _context.DonViTinhs.RemoveRange(existingUnits);
                    
                    if (units != null)
                    {
                        foreach (var u in units)
                        {
                            if (!string.IsNullOrEmpty(u.TenDonVi))
                            {
                                u.Id = 0; // Reset ID to force insert
                                u.MaHang = id;
                                _context.Add(u);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HangHoaExists(hangHoa.MaHang))
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
            return View(hangHoa);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa != null)
            {
                _context.HangHoas.Remove(hangHoa);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SearchJson(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var products = await _context.HangHoas
                .Where(p => p.TenHang.Contains(term) || p.MaHang.Contains(term))
                .Take(20)
                .Select(p => new
                {
                    p.MaHang,
                    p.TenHang,
                    p.DonViTinh,
                    p.GiaBanHienTai,
                    p.GiaVonHienTai,
                    // Use a placeholder if no image
                    HinhAnh = "/images/product_placeholder.png" 
                })
                .ToListAsync();

            return Json(products);
        }

        private bool HangHoaExists(string id)
        {
            return _context.HangHoas.Any(e => e.MaHang == id);
        }
    }
}
