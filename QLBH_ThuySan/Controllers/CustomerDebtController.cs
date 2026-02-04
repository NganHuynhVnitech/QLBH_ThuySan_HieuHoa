using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for managing Customer Debt (Công Nợ Khách Hàng)
    /// Maps to SoRiengKhachHang table in HieuHoaDB
    /// </summary>
    [Authorize]
    public class CustomerDebtController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerDebtController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PrivateLedger - List all customers with their ledger entries
        public async Task<IActionResult> Index()
        {
            var customers = await _context.KhachHangs.ToListAsync();
            return View(customers);
        }

        // GET: PrivateLedger/Details/KH001 - View ledger entries for a specific customer
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.KhachHangs
                .FirstOrDefaultAsync(m => m.MaDoiTuong == id);

            if (customer == null)
            {
                return NotFound();
            }

            var entries = await _context.SoRiengKhachHangs
                .Where(e => e.MaKhachHang == id)
                .OrderByDescending(e => e.NgayGiaoDich)
                .ToListAsync();

            ViewBag.Entries = entries;
            return View(customer);
        }

        // GET: PrivateLedger/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrivateLedger/Create - Create a new customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDoiTuong,TenDoiTuong,SoDienThoai,DiaChi,AoNuoi")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                khachHang.DuNoLuyKe = 0;
                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // POST: PrivateLedger/AddEntry - Add a ledger entry for a customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(string maKhachHang, string loaiGiaoDich, decimal soTienPhatSinh, string? dienGiai)
        {
            var ledgerEntry = new SoRiengKhachHang
            {
                MaKhachHang = maKhachHang,
                NgayGiaoDich = DateTime.Now,
                LoaiGiaoDich = loaiGiaoDich,
                SoTienPhatSinh = soTienPhatSinh,
                DienGiai = dienGiai
            };

            _context.Add(ledgerEntry);

            // Update customer cumulative debt
            var customer = await _context.KhachHangs.FindAsync(maKhachHang);
            if (customer != null)
            {
                // If loaiGiaoDich is "No" (debt), add to balance; if "Thu" (payment), subtract
                if (loaiGiaoDich == "No")
                {
                    customer.DuNoLuyKe = (customer.DuNoLuyKe ?? 0) + soTienPhatSinh;
                }
                else if (loaiGiaoDich == "Thu")
                {
                    customer.DuNoLuyKe = (customer.DuNoLuyKe ?? 0) - soTienPhatSinh;
                }
                _context.Update(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = maKhachHang });
        }
    }
}
