using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class AgencyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgencyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Agency
        public async Task<IActionResult> Index()
        {
            return View(await _context.DaiLys.ToListAsync());
        }

        // GET: Agency/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daiLy = await _context.DaiLys
                .FirstOrDefaultAsync(m => m.MaDaiLy == id);
            if (daiLy == null)
            {
                return NotFound();
            }

            return View(daiLy);
        }

        // GET: Agency/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Agency/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDaiLy,TenDaiLy,LoaiDaiLy")] DaiLy daiLy)
        {
            if (ModelState.IsValid)
            {
                if (_context.DaiLys.Any(e => e.MaDaiLy == daiLy.MaDaiLy))
                {
                    ModelState.AddModelError("MaDaiLy", "Mã đại lý đã tồn tại.");
                    return View(daiLy);
                }

                _context.Add(daiLy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(daiLy);
        }

        // GET: Agency/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daiLy = await _context.DaiLys.FindAsync(id);
            if (daiLy == null)
            {
                return NotFound();
            }
            return View(daiLy);
        }

        // POST: Agency/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaDaiLy,TenDaiLy,LoaiDaiLy")] DaiLy daiLy)
        {
            if (id != daiLy.MaDaiLy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(daiLy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DaiLyExists(daiLy.MaDaiLy))
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
            return View(daiLy);
        }

        // POST: Agency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var daiLy = await _context.DaiLys.FindAsync(id);
            if (daiLy != null)
            {
                _context.DaiLys.Remove(daiLy);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DaiLyExists(string id)
        {
            return _context.DaiLys.Any(e => e.MaDaiLy == id);
        }
    }
}
