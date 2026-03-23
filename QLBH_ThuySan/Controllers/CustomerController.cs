using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(string? searchMa, string? searchTen, string? searchSdt, string? searchDiaChi, string? searchAoNuoi, bool searchCoNo = false, string? sortOrder = null)
        {
            var query = _context.KhachHangs.Where(k => !k.IsDisabled);

            if (!string.IsNullOrEmpty(searchMa))
            {
                query = query.Where(k => k.MaDoiTuong.Contains(searchMa));
            }

            if (!string.IsNullOrEmpty(searchTen))
            {
                query = query.Where(k => k.TenDoiTuong != null && k.TenDoiTuong.Contains(searchTen));
            }

            if (!string.IsNullOrEmpty(searchSdt))
            {
                query = query.Where(k => k.SoDienThoai != null && k.SoDienThoai.Contains(searchSdt));
            }
            
            if (!string.IsNullOrEmpty(searchDiaChi))
            {
                query = query.Where(k => k.DiaChi != null && k.DiaChi.Contains(searchDiaChi));
            }
            
            if (!string.IsNullOrEmpty(searchAoNuoi))
            {
                query = query.Where(k => k.AoNuoi != null && k.AoNuoi.Contains(searchAoNuoi));
            }
            
            if (searchCoNo)
            {
                query = query.Where(k => k.DuNoLuyKe > 0);
            }

            ViewData["searchMa"] = searchMa;
            ViewData["searchTen"] = searchTen;
            ViewData["searchSdt"] = searchSdt;
            ViewData["searchDiaChi"] = searchDiaChi;
            ViewData["searchAoNuoi"] = searchAoNuoi;
            ViewData["searchCoNo"] = searchCoNo;

            // Sorting Parameters
            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ma_desc" : "";
            ViewData["TenSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["SdtSortParm"] = sortOrder == "sdt_asc" ? "sdt_desc" : "sdt_asc";
            ViewData["DiaChiSortParm"] = sortOrder == "addr_asc" ? "addr_desc" : "addr_asc";
            ViewData["AoNuoiSortParm"] = sortOrder == "pond_asc" ? "pond_desc" : "pond_asc";
            ViewData["DebtSortParm"] = sortOrder == "debt_asc" ? "debt_desc" : "debt_asc";

            query = sortOrder switch
            {
                "ma_desc" => query.OrderByDescending(k => k.MaDoiTuong),
                "name_asc" => query.OrderBy(k => k.TenDoiTuong),
                "name_desc" => query.OrderByDescending(k => k.TenDoiTuong),
                "sdt_asc" => query.OrderBy(k => k.SoDienThoai),
                "sdt_desc" => query.OrderByDescending(k => k.SoDienThoai),
                "addr_asc" => query.OrderBy(k => k.DiaChi),
                "addr_desc" => query.OrderByDescending(k => k.DiaChi),
                "pond_asc" => query.OrderBy(k => k.AoNuoi),
                "pond_desc" => query.OrderByDescending(k => k.AoNuoi),
                "debt_asc" => query.OrderBy(k => k.DuNoLuyKe),
                "debt_desc" => query.OrderByDescending(k => k.DuNoLuyKe),
                _ => query.OrderBy(k => k.MaDoiTuong),
            };

            return View(await query.ToListAsync());
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.MaDoiTuong == id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDoiTuong,TenDoiTuong,SoDienThoai,DiaChi,AoNuoi")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                // Check if ID exists
                if (_context.KhachHangs.Any(e => e.MaDoiTuong == khachHang.MaDoiTuong))
                {
                    ModelState.AddModelError("MaDoiTuong", "Mã khách hàng đã tồn tại.");
                    return View(khachHang);
                }

                khachHang.DuNoLuyKe = 0; // Initialize debt to 0
                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }
            return View(khachHang);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaDoiTuong,TenDoiTuong,SoDienThoai,DiaChi,AoNuoi")] KhachHang khachHang)
        {
            if (id != khachHang.MaDoiTuong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve existing debt
                    var existingCustomer = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.MaDoiTuong == id);
                    if (existingCustomer != null)
                    {
                        khachHang.DuNoLuyKe = existingCustomer.DuNoLuyKe;
                    }

                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachHangExists(khachHang.MaDoiTuong))
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
            return View(khachHang);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang != null)
            {
                khachHang.IsDisabled = true;
                _context.Update(khachHang);
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

            var customers = await _context.KhachHangs
                .Where(c => !c.IsDisabled && ((c.TenDoiTuong != null && c.TenDoiTuong.Contains(term)) || c.MaDoiTuong.Contains(term) || (c.SoDienThoai != null && c.SoDienThoai.Contains(term))))
                .Take(20)
                .Select(c => new
                {
                    c.MaDoiTuong,
                    c.TenDoiTuong,
                    c.SoDienThoai,
                    c.DiaChi
                })
                .ToListAsync();

            return Json(customers);
        }

        [HttpPost]
        public async Task<IActionResult> QuickCreate([FromBody] KhachHang khachHang)
        {
            if (khachHang == null) return BadRequest("Invalid Data");

            // Basic validation
            if (string.IsNullOrEmpty(khachHang.TenDoiTuong)) return BadRequest("Tên khách hàng là bắt buộc");
            
            // Generate ID if missing (Simple logic: KH + Random or Timestamp for MVP)
            if (string.IsNullOrEmpty(khachHang.MaDoiTuong))
            {
                khachHang.MaDoiTuong = "KH" + DateTime.Now.ToString("yyMMddHHmmss");
            }

            if (_context.KhachHangs.Any(e => e.MaDoiTuong == khachHang.MaDoiTuong))
            {
                return BadRequest("Mã khách hàng đã tồn tại");
            }

            khachHang.DuNoLuyKe = 0;
            try
            {
                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return Json(new { success = true, data = khachHang });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi lưu: " + ex.Message);
            }
        }

        private bool KhachHangExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaDoiTuong == id);
        }
    }
}
