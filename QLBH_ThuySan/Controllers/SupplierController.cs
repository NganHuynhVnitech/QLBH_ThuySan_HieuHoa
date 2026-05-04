using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class SupplierController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: Supplier
        public async Task<IActionResult> Index(string? searchMa, string? searchTen, string? searchSdt, string? searchDiaChi, bool searchCoNo = false, string? sortOrder = null)
        {
            var query = _context.NhaCungCaps.Where(s => !s.IsDisabled);

            if (!string.IsNullOrEmpty(searchMa))
            {
                query = query.Where(s => s.MaDoiTuong != null && s.MaDoiTuong.Contains(searchMa));
            }

            if (!string.IsNullOrEmpty(searchTen))
            {
                query = query.Where(s => s.TenDoiTuong != null && s.TenDoiTuong.Contains(searchTen));
            }

            if (!string.IsNullOrEmpty(searchSdt))
            {
                query = query.Where(s => s.SoDienThoai != null && s.SoDienThoai.Contains(searchSdt));
            }

            if (!string.IsNullOrEmpty(searchDiaChi))
            {
                query = query.Where(s => s.DiaChi != null && s.DiaChi.Contains(searchDiaChi));
            }

            if (searchCoNo)
            {
                query = query.Where(s => s.DuNoLuyKe > 0);
            }

            ViewData["searchMa"] = searchMa;
            ViewData["searchTen"] = searchTen;
            ViewData["searchSdt"] = searchSdt;
            ViewData["searchDiaChi"] = searchDiaChi;
            ViewData["searchCoNo"] = searchCoNo;

            // Sorting Parameters
            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ma_desc" : "";
            ViewData["TenSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["SdtSortParm"] = sortOrder == "sdt_asc" ? "sdt_desc" : "sdt_asc";
            ViewData["DiaChiSortParm"] = sortOrder == "addr_asc" ? "addr_desc" : "addr_asc";
            ViewData["TaxSortParm"] = sortOrder == "tax_asc" ? "tax_desc" : "tax_asc";
            ViewData["DaySortParm"] = sortOrder == "day_asc" ? "day_desc" : "day_asc";
            ViewData["DebtSortParm"] = sortOrder == "debt_asc" ? "debt_desc" : "debt_asc";

            query = sortOrder switch
            {
                "ma_desc" => query.OrderByDescending(s => s.MaDoiTuong),
                "name_asc" => query.OrderBy(s => s.TenDoiTuong),
                "name_desc" => query.OrderByDescending(s => s.TenDoiTuong),
                "sdt_asc" => query.OrderBy(s => s.SoDienThoai),
                "sdt_desc" => query.OrderByDescending(s => s.SoDienThoai),
                "addr_asc" => query.OrderBy(s => s.DiaChi),
                "addr_desc" => query.OrderByDescending(s => s.DiaChi),
                "tax_asc" => query.OrderBy(s => s.MaSoThue),
                "tax_desc" => query.OrderByDescending(s => s.MaSoThue),
                "day_asc" => query.OrderBy(s => s.SoNgayDuocNo),
                "day_desc" => query.OrderByDescending(s => s.SoNgayDuocNo),
                "debt_asc" => query.OrderBy(s => s.DuNoLuyKe),
                "debt_desc" => query.OrderByDescending(s => s.DuNoLuyKe),
                _ => query.OrderBy(s => s.MaDoiTuong),
            };

            return View(await query.ToListAsync());
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
        public async Task<IActionResult> Create()
        {
            var model = new NhaCungCap
            {
                MaDoiTuong = await _codeGen.GenerateSupplierCodeAsync()
            };
            return View(model);
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
                nhaCungCap.IsDisabled = true;
                _context.Update(nhaCungCap);
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
                .Where(s => !s.IsDisabled && (
                    (s.TenDoiTuong != null && s.TenDoiTuong.Contains(term)) || 
                    (s.MaDoiTuong != null && s.MaDoiTuong.Contains(term)) || 
                    (s.SoDienThoai != null && s.SoDienThoai.Contains(term))
                ))
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
                nhaCungCap.MaDoiTuong = await _codeGen.GenerateSupplierCodeAsync();
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
