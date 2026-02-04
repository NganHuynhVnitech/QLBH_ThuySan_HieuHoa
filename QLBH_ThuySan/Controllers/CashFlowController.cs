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
            var items = await _context.PhieuThuChis
                .OrderByDescending(p => p.NgayLap)
                .ToListAsync();
            return View(items);
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
            return View();
        }

        // POST: CashFlow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieu,LoaiPhieu,NgayLap,SoTien,LyDo")] PhieuThuChi phieuThuChi)
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
