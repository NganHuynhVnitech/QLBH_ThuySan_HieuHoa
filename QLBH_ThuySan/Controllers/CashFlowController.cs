using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class CashFlowController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashFlowController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CashFlow
        public async Task<IActionResult> Index()
        {
            // Pre-load names for display
            ViewData["CustomerNames"] = await _context.KhachHangs.ToDictionaryAsync(k => k.MaDoiTuong, k => k.TenDoiTuong);
            ViewData["SupplierNames"] = await _context.NhaCungCaps.ToDictionaryAsync(n => n.MaDoiTuong, n => n.TenDoiTuong);
            ViewData["CostObjectNames"] = await _context.DoiTuongChiPhis.ToDictionaryAsync(d => d.MaDoiTuong, d => d.TenDoiTuong);

            return View(await _context.PhieuThuChis.OrderByDescending(p => p.NgayLap).ToListAsync());
        }

        // GET: CashFlow/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuThuChi = await _context.PhieuThuChis
                .FirstOrDefaultAsync(m => m.MaPhieu == id);
            if (phieuThuChi == null)
            {
                return NotFound();
            }

            return View(phieuThuChi);
        }

        // GET: CashFlow/Create
        public IActionResult Create()
        {
            ViewData["Customers"] = _context.KhachHangs.Select(k => new { k.MaDoiTuong, k.TenDoiTuong }).ToList();
            ViewData["Suppliers"] = _context.NhaCungCaps.Select(n => new { n.MaDoiTuong, n.TenDoiTuong }).ToList();
            ViewData["CostObjects"] = _context.DoiTuongChiPhis.Select(d => new { d.MaDoiTuong, d.TenDoiTuong }).ToList();
            return View();
        }

        // POST: CashFlow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieu,LoaiPhieu,NgayLap,SoTien,LyDo,MaDoiTuong")] PhieuThuChi phieuThuChi)
        {
            if (ModelState.IsValid)
            {
                if (_context.PhieuThuChis.Any(e => e.MaPhieu == phieuThuChi.MaPhieu))
                {
                    ModelState.AddModelError("MaPhieu", "Mã phiếu đã tồn tại.");
                    return View(phieuThuChi);
                }

                _context.Add(phieuThuChi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phieuThuChi);
        }

        // GET: CashFlow/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
             if (id == null)
            {
                return NotFound();
            }

            var phieuThuChi = await _context.PhieuThuChis
                .FirstOrDefaultAsync(m => m.MaPhieu == id);
            if (phieuThuChi == null)
            {
                return NotFound();
            }

            return View(phieuThuChi);
        }

        // POST: CashFlow/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phieuThuChi = await _context.PhieuThuChis.FindAsync(id);
            if (phieuThuChi != null)
            {
                _context.PhieuThuChis.Remove(phieuThuChi);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
