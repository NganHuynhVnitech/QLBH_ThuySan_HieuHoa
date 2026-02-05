using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Supplier
        public async Task<IActionResult> Index()
        {
            return View(await _context.NhaCungCaps.ToListAsync());
        }

        // GET: Supplier/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCaps
                .FirstOrDefaultAsync(m => m.MaDoiTuong == id);
            if (nhaCungCap == null)
            {
                return NotFound();
            }

            return View(nhaCungCap);
        }

        // GET: Supplier/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDoiTuong,TenDoiTuong,SoDienThoai,DiaChi,MaSoThue,SoNgayDuocNo")] NhaCungCap nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                // Check if ID exists
                if (_context.NhaCungCaps.Any(e => e.MaDoiTuong == nhaCungCap.MaDoiTuong))
                {
                    ModelState.AddModelError("MaDoiTuong", "Mã nhà cung cấp đã tồn tại.");
                    return View(nhaCungCap);
                }

                _context.Add(nhaCungCap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nhaCungCap);
        }

        // GET: Supplier/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCaps.FindAsync(id);
            if (nhaCungCap == null)
            {
                return NotFound();
            }
            return View(nhaCungCap);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaDoiTuong,TenDoiTuong,SoDienThoai,DiaChi,MaSoThue,SoNgayDuocNo")] NhaCungCap nhaCungCap)
        {
            if (id != nhaCungCap.MaDoiTuong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhaCungCap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhaCungCapExists(nhaCungCap.MaDoiTuong))
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
            return View(nhaCungCap);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhaCungCap = await _context.NhaCungCaps.FindAsync(id);
            if (nhaCungCap != null)
            {
                _context.NhaCungCaps.Remove(nhaCungCap);
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

            var suppliers = await _context.NhaCungCaps
                .Where(s => s.TenDoiTuong.Contains(term) || s.MaDoiTuong.Contains(term) || s.SoDienThoai.Contains(term))
                .Take(20)
                .Select(s => new
                {
                    s.MaDoiTuong,
                    s.TenDoiTuong,
                    s.SoDienThoai,
                    s.DiaChi
                })
                .ToListAsync();

            return Json(suppliers);
        }

        [HttpPost]
        public async Task<IActionResult> QuickCreate([FromBody] NhaCungCap nhaCungCap)
        {
            if (nhaCungCap == null) return BadRequest("Invalid Data");

            if (string.IsNullOrEmpty(nhaCungCap.TenDoiTuong)) return BadRequest("Tên nhà cung cấp là bắt buộc");
            
            if (string.IsNullOrEmpty(nhaCungCap.MaDoiTuong))
            {
                nhaCungCap.MaDoiTuong = "NCC" + DateTime.Now.ToString("yyMMddHHmmss");
            }

            if (_context.NhaCungCaps.Any(e => e.MaDoiTuong == nhaCungCap.MaDoiTuong))
            {
                return BadRequest("Mã nhà cung cấp đã tồn tại");
            }

            try
            {
                _context.Add(nhaCungCap);
                await _context.SaveChangesAsync();
                return Json(new { success = true, data = nhaCungCap });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi lưu: " + ex.Message);
            }
        }

        private bool NhaCungCapExists(string id)
        {
            return _context.NhaCungCaps.Any(e => e.MaDoiTuong == id);
        }
    }
}
