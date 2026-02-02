using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    public class PrivateLedgerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrivateLedgerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PrivateLedger
        public async Task<IActionResult> Index()
        {
            return View(await _context.PrivateLedgers.ToListAsync());
        }

        // GET: PrivateLedger/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var privateLedger = await _context.PrivateLedgers
                .FirstOrDefaultAsync(m => m.LedgerId == id);

            if (privateLedger == null)
            {
                return NotFound();
            }

            var entries = await _context.LedgerEntries
                .Where(e => e.LedgerId == id)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();

            ViewBag.Entries = entries;
            return View(privateLedger);
        }

        // GET: PrivateLedger/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrivateLedger/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LedgerCode,LedgerType,Description")] PrivateLedger privateLedger)
        {
            if (ModelState.IsValid)
            {
                _context.Add(privateLedger);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(privateLedger);
        }

        // POST: PrivateLedger/AddEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(int ledgerId, string entryCode, decimal debtAmount, decimal paymentAmount, string? notes)
        {
            var ledgerEntry = new LedgerEntry
            {
                LedgerId = ledgerId,
                EntryCode = entryCode,
                EntryDate = DateTime.Now,
                DebtAmount = debtAmount,
                PaymentAmount = paymentAmount,
                Notes = notes
            };

            _context.Add(ledgerEntry);

            // Update ledger totals
            var ledger = await _context.PrivateLedgers.FindAsync(ledgerId);
            if (ledger != null)
            {
                ledger.TotalDebt += debtAmount;
                ledger.TotalPaid += paymentAmount;
                _context.Update(ledger);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = ledgerId });
        }
    }
}
