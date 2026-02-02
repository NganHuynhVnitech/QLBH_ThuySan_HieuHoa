using Microsoft.EntityFrameworkCore;

namespace QLBH_ThuySan.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductWarehouse> ProductWarehouses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<PrivateLedger> PrivateLedgers { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<SupplierDiscount> SupplierDiscounts { get; set; }
        public DbSet<CustomerDiscount> CustomerDiscounts { get; set; }
        public DbSet<DiscountTier> DiscountTiers { get; set; }
        public DbSet<OperatingExpense> OperatingExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique indexes
            modelBuilder.Entity<Warehouse>()
                .HasIndex(w => w.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductCode)
                .IsUnique();

            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.SupplierCode)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CustomerCode)
                .IsUnique();

            modelBuilder.Entity<ProductWarehouse>()
                .HasIndex(pw => new { pw.ProductId, pw.WarehouseId })
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<ProductWarehouse>()
                .Property(pw => pw.WeightedAverageCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.UnitPrice)
                .HasPrecision(18, 2);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed 3 physical warehouses + 1 virtual warehouse
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { WarehouseId = 1, Name = "Kho 1 - Khu A", Location = "123 Đường ABC, Quận 1", IsVirtual = false, CreatedDate = DateTime.Now },
                new Warehouse { WarehouseId = 2, Name = "Kho 2 - Khu B", Location = "456 Đường DEF, Quận 2", IsVirtual = false, CreatedDate = DateTime.Now },
                new Warehouse { WarehouseId = 3, Name = "Kho 3 - Khu C", Location = "789 Đường GHI, Quận 3", IsVirtual = false, CreatedDate = DateTime.Now },
                new Warehouse { WarehouseId = 4, Name = "Kho Tổng Ảo", Location = "Hệ thống tổng hợp", IsVirtual = true, CreatedDate = DateTime.Now }
            );

            // Seed some sample products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductCode = "TS001", ProductName = "Tôm sú", Description = "Tôm sú loại 1", Unit = "kg", BasePrice = 350000, CreatedDate = DateTime.Now },
                new Product { ProductId = 2, ProductCode = "TS002", ProductName = "Cá hồi", Description = "Cá hồi Na Uy", Unit = "kg", BasePrice = 450000, CreatedDate = DateTime.Now },
                new Product { ProductId = 3, ProductCode = "TS003", ProductName = "Mực ống", Description = "Mực ống tươi", Unit = "kg", BasePrice = 280000, CreatedDate = DateTime.Now }
            );
        }
    }
}
