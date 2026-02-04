using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class DiscountCalculationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountCalculationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DiscountCalculation
        public async Task<IActionResult> Index()
        {
            var calculations = await _context.PhieuTinhChietKhaus
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();
            return View(calculations);
        }

        // GET: DiscountCalculation/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieu = await _context.PhieuTinhChietKhaus
                .Include(p => p.BangKeChietKhaus) // Load results
                .FirstOrDefaultAsync(m => m.MaPhieuTinh == id);

            if (phieu == null)
            {
                return NotFound();
            }

            // Need to load Name of DoiTuong for display. 
            // BangKeChietKhau only has MaDoiTuong.
            // We can fetch names separately or use a ViewModel.
            // For simplicity, we'll list MaDoiTuong.
            // Or fetch lookup dictionaries.
            
            var maDoiTuongs = phieu.BangKeChietKhaus.Select(b => b.MaDoiTuong).Distinct().ToList();
            if (phieu.LoaiDoiTuong == "NCC")
            {
                var nccs = await _context.NhaCungCaps.Where(n => maDoiTuongs.Contains(n.MaDoiTuong)).ToDictionaryAsync(n => n.MaDoiTuong, n => n.TenDoiTuong);
                ViewBag.Names = nccs;
            }
            else
            {
                var khs = await _context.KhachHangs.Where(k => maDoiTuongs.Contains(k.MaDoiTuong)).ToDictionaryAsync(k => k.MaDoiTuong, k => k.TenDoiTuong);
                ViewBag.Names = khs;
            }

            return View(phieu);
        }

        // GET: DiscountCalculation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DiscountCalculation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuTinh,LoaiDoiTuong,TuNgay,DenNgay")] PhieuTinhChietKhau phieu)
        {
            if (ModelState.IsValid)
            {
                // Logic check: duplicates
                if (_context.PhieuTinhChietKhaus.Any(e => e.MaPhieuTinh == phieu.MaPhieuTinh))
                {
                    ModelState.AddModelError("MaPhieuTinh", "Mã phiếu đã tồn tại.");
                    return View(phieu);
                }

                _context.Add(phieu);
                await _context.SaveChangesAsync();

                // Call Engine SP
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_Engine_TinhChietKhau @p0", phieu.MaPhieuTinh);

                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieuTinh });
            }
            return View(phieu);
        }
    }
}
