using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public class ProfitLossReport
    {
        public decimal TotalRevenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit => TotalRevenue - CostOfGoodsSold;
        public decimal DiscountReceived { get; set; }
        public decimal DiscountGiven { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal NetProfit => GrossProfit + DiscountReceived - DiscountGiven - OperatingExpenses;
    }

    public interface IFinancialService
    {
        Task<ProfitLossReport> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate);
    }

    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _context;

        public FinancialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfitLossReport> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new ProfitLossReport();

            // Calculate total revenue from sales orders
            var salesOrders = _context.SalesOrders
                .Where(so => so.OrderDate >= startDate 
                    && so.OrderDate <= endDate 
                    && so.Status == OrderStatus.Completed)
                .ToList();

            report.TotalRevenue = salesOrders.Sum(so => so.FinalAmount);
            report.CostOfGoodsSold = salesOrders.Sum(so => so.CostOfGoodsSold);
            report.DiscountGiven = salesOrders.Sum(so => so.DiscountAmount);

            // Calculate discount received from purchase orders
            var purchaseOrders = _context.PurchaseOrders
                .Where(po => po.OrderDate >= startDate 
                    && po.OrderDate <= endDate 
                    && po.Status == OrderStatus.Completed)
                .ToList();

            report.DiscountReceived = purchaseOrders.Sum(po => po.DiscountAmount);

            // Calculate operating expenses
            var expenses = _context.OperatingExpenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .ToList();

            report.OperatingExpenses = expenses.Sum(e => e.Amount);

            return report;
        }
    }
}
