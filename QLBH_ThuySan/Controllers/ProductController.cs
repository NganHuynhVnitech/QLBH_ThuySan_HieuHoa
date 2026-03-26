using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class ProductController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: Product
        public async Task<IActionResult> Index(string? searchMa, string? searchTen, string? sortOrder = null)
        {
            var query = _context.HangHoas.Where(h => !h.IsDisabled);

            if (!string.IsNullOrEmpty(searchMa))
            {
                query = query.Where(h => h.MaHang != null && h.MaHang.Contains(searchMa));
            }

            if (!string.IsNullOrEmpty(searchTen))
            {
                query = query.Where(h => h.TenHang != null && h.TenHang.Contains(searchTen));
            }

            ViewData["searchMa"] = searchMa;
            ViewData["searchTen"] = searchTen;

            // Sorting Parameters
            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ma_desc" : "";
            ViewData["TenSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["DvtSortParm"] = sortOrder == "unit_asc" ? "unit_desc" : "unit_asc";
            ViewData["QcSortParm"] = sortOrder == "qc_asc" ? "qc_desc" : "qc_asc";
            ViewData["VonSortParm"] = sortOrder == "cost_asc" ? "cost_desc" : "cost_asc";
            ViewData["BanSortParm"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            query = sortOrder switch
            {
                "ma_desc" => query.OrderByDescending(h => h.MaHang),
                "name_asc" => query.OrderBy(h => h.TenHang),
                "name_desc" => query.OrderByDescending(h => h.TenHang),
                "unit_asc" => query.OrderBy(h => h.DonViTinh),
                "unit_desc" => query.OrderByDescending(h => h.DonViTinh),
                "qc_asc" => query.OrderBy(h => h.QuyCach),
                "qc_desc" => query.OrderByDescending(h => h.QuyCach),
                "cost_asc" => query.OrderBy(h => h.GiaVonHienTai),
                "cost_desc" => query.OrderByDescending(h => h.GiaVonHienTai),
                "price_asc" => query.OrderBy(h => h.GiaBanHienTai),
                "price_desc" => query.OrderByDescending(h => h.GiaBanHienTai),
                _ => query.OrderBy(h => h.MaHang),
            };

            return View(await query.ToListAsync());
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
        public async Task<IActionResult> Create()
        {
            var model = new HangHoa
            {
                MaHang = await _codeGen.GenerateProductCodeAsync()
            };
            return View(model);
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
                hangHoa.IsDisabled = true;
                _context.Update(hangHoa);
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
                .Where(p => !p.IsDisabled && (p.TenHang.Contains(term) || p.MaHang.Contains(term)))
                .Take(20)
                .Select(p => new
                {
                    p.MaHang,
                    p.TenHang,
                    p.DonViTinh,
                    p.GiaBanHienTai,
                    p.GiaVonHienTai,
                    TonKho = p.ChiTietTons.Sum(t => t.SoLuongTon ?? 0),
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
