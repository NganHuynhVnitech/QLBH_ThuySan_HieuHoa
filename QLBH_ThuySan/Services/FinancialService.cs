using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public class ProfitLossReport
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal GrossProfit => TotalRevenue - TotalPurchases;
        public decimal TotalExpenses { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal NetProfit => GrossProfit - TotalExpenses + TotalReceipts;
    }

    public interface IFinancialService
    {
        Task<ProfitLossReport> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Service for financial reporting
    /// Uses PhieuNhap, PhieuXuat, and PhieuThuChi from HieuHoaDB
    /// </summary>
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

            // Calculate total revenue from sales orders (PhieuXuat)
            var salesOrders = await _context.PhieuXuats
                .Where(px => px.NgayXuat >= startDate && px.NgayXuat <= endDate)
                .ToListAsync();

            report.TotalRevenue = salesOrders.Sum(px => px.SoPhaiThanhToan ?? 0);

            // Calculate total purchases from purchase orders (PhieuNhap)
            var purchaseOrders = await _context.PhieuNhaps
                .Where(pn => pn.NgayNhap >= startDate && pn.NgayNhap <= endDate)
                .ToListAsync();

            report.TotalPurchases = purchaseOrders.Sum(pn => pn.SoPhaiThanhToan ?? 0);

            // Calculate expenses and receipts from cash receipts/payments (PhieuThuChi)
            var cashTransactions = await _context.PhieuThuChis
                .Where(ptc => ptc.NgayLap >= startDate && ptc.NgayLap <= endDate)
                .ToListAsync();

            report.TotalExpenses = cashTransactions
                .Where(ptc => ptc.LoaiPhieu == "Chi")
                .Sum(ptc => ptc.SoTien ?? 0);

            report.TotalReceipts = cashTransactions
                .Where(ptc => ptc.LoaiPhieu == "Thu")
                .Sum(ptc => ptc.SoTien ?? 0);

            return report;
        }
    }
}

