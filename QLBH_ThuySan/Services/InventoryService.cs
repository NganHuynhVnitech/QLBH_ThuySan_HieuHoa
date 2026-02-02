using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public interface IInventoryService
    {
        Task<decimal> CalculateWeightedAverageCostAsync(int productId, int warehouseId, decimal quantity, decimal unitPrice);
        Task<ProductWarehouse?> GetProductWarehouseAsync(int productId, int warehouseId);
    }

    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateWeightedAverageCostAsync(int productId, int warehouseId, decimal quantity, decimal unitPrice)
        {
            var newWACParam = new SqlParameter
            {
                ParameterName = "@NewWeightedAverageCost",
                SqlDbType = System.Data.SqlDbType.Decimal,
                Direction = System.Data.ParameterDirection.Output,
                Precision = 18,
                Scale = 2
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC @NewWeightedAverageCost = sp_CalculateWeightedAverageCost @ProductId, @WarehouseId, @NewQuantity, @NewUnitPrice",
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@WarehouseId", warehouseId),
                new SqlParameter("@NewQuantity", quantity),
                new SqlParameter("@NewUnitPrice", unitPrice),
                newWACParam
            );

            return (decimal)(newWACParam.Value ?? 0);
        }

        public async Task<ProductWarehouse?> GetProductWarehouseAsync(int productId, int warehouseId)
        {
            return await _context.ProductWarehouses
                .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);
        }
    }
}
