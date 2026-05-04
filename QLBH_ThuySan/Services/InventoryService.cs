using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public interface IInventoryService
    {
        Task<decimal> CalculateWeightedAverageCostAsync(string maHang, string maKho, double quantity, decimal unitPrice);
        Task<ChiTietTon?> GetInventoryAsync(string maHang, string maKho);
    }

    /// <summary>
    /// Service for inventory management operations
    /// Maps to ChiTietTon table in HieuHoaDB
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculate weighted average cost for a product in a warehouse
        /// Uses stored procedure sp_CalculateWeightedAverageCost if available
        /// </summary>
        public async Task<decimal> CalculateWeightedAverageCostAsync(string maHang, string maKho, double quantity, decimal unitPrice)
        {
            // Get current inventory
            var inventory = await GetInventoryAsync(maHang, maKho);
            
            if (inventory == null)
            {
                // No existing inventory, cost is the unit price
                return unitPrice;
            }

            // Calculate weighted average cost
            // WAC = (CurrentValue + NewValue) / (CurrentQty + NewQty)
            var currentQty = inventory.SoLuongTon ?? 0;
            var currentValue = inventory.GiaTriTon ?? 0;
            var newValue = (decimal)quantity * unitPrice;
            var totalQty = currentQty + quantity;

            if (totalQty <= 0)
            {
                return 0;
            }

            return (currentValue + newValue) / (decimal)totalQty;
        }

        /// <summary>
        /// Get inventory for a specific product in a specific warehouse
        /// </summary>
        public async Task<ChiTietTon?> GetInventoryAsync(string maHang, string maKho)
        {
            return await _context.ChiTietTons
                .Include(ct => ct.MaHangNavigation)
                .Include(ct => ct.MaKhoNavigation)
                .FirstOrDefaultAsync(ct => ct.MaHang == maHang && ct.MaKho == maKho);
        }
    }
}
