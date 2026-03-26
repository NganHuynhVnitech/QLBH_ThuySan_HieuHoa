
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;

namespace QLBH_ThuySan.Controllers
{
    public class CostObjectController(ApplicationDbContext context, ICodeGenerationService codeGen) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ICodeGenerationService _codeGen = codeGen;

        // GET: CostObject
        public async Task<IActionResult> Index()
        {
            return View(await _context.DoiTuongChiPhis.Where(x => !x.IsDisabled).ToListAsync());
        }

        // GET: CostObject/Create
        public async Task<IActionResult> Create()
        {
            var model = new DoiTuongChiPhi
            {
                MaDoiTuong = await _codeGen.GenerateCostObjectCodeAsync()
            };
            return View(model);
        }

        // POST: CostObject/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDoiTuong,TenDoiTuong")] DoiTuongChiPhi doiTuongChiPhi)
        {
            if (ModelState.IsValid)
            {
                // Check duplicate
                if (await _context.DoiTuongChiPhis.AnyAsync(x => x.MaDoiTuong == doiTuongChiPhi.MaDoiTuong))
                {
                    ModelState.AddModelError("MaDoiTuong", "Mã đối tượng đã tồn tại.");
                    return View(doiTuongChiPhi);
                }

                _context.Add(doiTuongChiPhi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doiTuongChiPhi);
        }

        // GET: CostObject/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doiTuongChiPhi = await _context.DoiTuongChiPhis.FindAsync(id);
            if (doiTuongChiPhi == null)
            {
                return NotFound();
            }
            return View(doiTuongChiPhi);
        }

        // POST: CostObject/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaDoiTuong,TenDoiTuong")] DoiTuongChiPhi doiTuongChiPhi)
        {
            if (id != doiTuongChiPhi.MaDoiTuong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doiTuongChiPhi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoiTuongChiPhiExists(doiTuongChiPhi.MaDoiTuong))
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
            return View(doiTuongChiPhi);
        }

        // POST: CostObject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var doiTuongChiPhi = await _context.DoiTuongChiPhis.FindAsync(id);
            if (doiTuongChiPhi != null)
            {
                doiTuongChiPhi.IsDisabled = true;
                _context.Update(doiTuongChiPhi);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DoiTuongChiPhiExists(string id)
        {
            return _context.DoiTuongChiPhis.Any(e => e.MaDoiTuong == id);
        }
    }
}
