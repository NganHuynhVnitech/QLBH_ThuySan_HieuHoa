using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var thresholdDate = today.AddDays(3);

        var warnings = await _context.PhieuNhaps
            .Include(p => p.IdNhaCungCapNavigation)
            .Where(p => !p.IsDisabled && 
                        p.TrangThaiThanhToan != "Đã Thanh Toán" && 
                        p.HanThanhToan != null && 
                        p.HanThanhToan <= thresholdDate)
            .OrderBy(p => p.HanThanhToan)
            .Take(10) // Limit to top 10
            .ToListAsync();

        ViewBag.PaymentWarnings = warnings;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
