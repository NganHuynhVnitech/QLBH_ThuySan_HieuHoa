using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class DiscountConfigController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DiscountConfig
        public async Task<IActionResult> Index()
        {
            return View(await _context.CauHinhChietKhaus.ToListAsync());
        }

        // GET: DiscountConfig/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DiscountConfig/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenCauHinh,LoaiQuyLuat,DoiTuongApDung,ChiTietLuat")] CauHinhChietKhau cauHinh)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cauHinh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cauHinh);
        }

        // GET: DiscountConfig/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cauHinh = await _context.CauHinhChietKhaus.FindAsync(id);
            if (cauHinh == null)
            {
                return NotFound();
            }
            return View(cauHinh);
        }

        // POST: DiscountConfig/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenCauHinh,LoaiQuyLuat,DoiTuongApDung,ChiTietLuat")] CauHinhChietKhau cauHinh)
        {
            if (id != cauHinh.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cauHinh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CauHinhExists(cauHinh.Id))
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
            return View(cauHinh);
        }

        // POST: DiscountConfig/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cauHinh = await _context.CauHinhChietKhaus.FindAsync(id);
            if (cauHinh != null)
            {
                _context.CauHinhChietKhaus.Remove(cauHinh);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CauHinhExists(int id)
        {
            return _context.CauHinhChietKhaus.Any(e => e.Id == id);
        }
    }
}
