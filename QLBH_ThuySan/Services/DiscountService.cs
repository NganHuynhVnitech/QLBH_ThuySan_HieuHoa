using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public interface IDiscountService
    {
        Task<decimal> CalculateDiscountAsync(string loaiDoiTuong, string maDoiTuong, decimal orderAmount);
        Task<IEnumerable<CauHinhChietKhau>> GetDiscountConfigurationsAsync(string doiTuongApDung);
    }

    /// <summary>
    /// Service for discount calculations
    /// Uses CauHinhChietKhau from HieuHoaDB
    /// </summary>
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculate discount based on configured rules
        /// </summary>
        public async Task<decimal> CalculateDiscountAsync(string loaiDoiTuong, string maDoiTuong, decimal orderAmount)
        {
            // Get applicable discount configurations
            var configs = await _context.CauHinhChietKhaus
                .Where(c => c.DoiTuongApDung == loaiDoiTuong)
                .ToListAsync();

            decimal totalDiscount = 0;

            foreach (var config in configs)
            {
                // Parse the ChiTietLuat (rule details) to calculate discount
                // The rule format depends on business logic stored in the database
                if (!string.IsNullOrEmpty(config.ChiTietLuat))
                {
                    // Simple implementation: assume ChiTietLuat contains percentage as JSON
                    // In real implementation, parse the rule and apply logic
                    if (decimal.TryParse(config.ChiTietLuat, out decimal percentage))
                    {
                        totalDiscount += orderAmount * (percentage / 100);
                    }
                }
            }

            return totalDiscount;
        }

        /// <summary>
        /// Get all discount configurations for a specific target type
        /// </summary>
        public async Task<IEnumerable<CauHinhChietKhau>> GetDiscountConfigurationsAsync(string doiTuongApDung)
        {
            return await _context.CauHinhChietKhaus
                .Where(c => c.DoiTuongApDung == doiTuongApDung)
                .ToListAsync();
        }
    }
}
