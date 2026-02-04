using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLBH_ThuySan.Services;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class FinancialController : Controller
    {
        private readonly IFinancialService _financialService;

        public FinancialController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        // GET: Financial/ProfitLoss
        public IActionResult ProfitLoss()
        {
            return View();
        }

        // POST: Financial/GenerateProfitLoss
        [HttpPost]
        public async Task<IActionResult> GenerateProfitLoss(DateTime startDate, DateTime endDate)
        {
            var report = await _financialService.GenerateProfitLossReportAsync(startDate, endDate);
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View("ProfitLossReport", report);
        }
    }
}
