# Copilot Instructions - QLBH Thủy Sản Hiệu Hoa

**Project**: Aquaculture Business Management System (QLBH_ThuySan_HieuHoa)  
**Stack**: ASP.NET Core MVC 10.0 + Entity Framework Core 10.0 + SQL Server  
**Language**: C# 12, SQL, Razor Views (Vietnamese context)

---

## Architecture Overview

### Layered Design
```
Presentation (Razor Views) 
  → MVC Controllers 
  → Service Layer (IDiscountService, IFinancialService, IInventoryService)
  → Data Access (ApplicationDbContext via EF Core)
  → SQL Server + Stored Procedures
```

### Key Design Patterns
- **Service Layer Pattern**: All business logic in [Services/](Services/) (DiscountService, FinancialService, InventoryService)
- **Repository Pattern**: Via EF Core DbContext with IQueryable
- **Dependency Injection**: ASP.NET Core built-in DI configured in [Program.cs](Program.cs)
- **Domain-Driven**: Models in [Models/](Models/) with enums (DiscountType, OrderStatus, LedgerType)

---

## Core Features & Implementation

### 1. Multi-Warehouse with Virtual Aggregation
**Files**: [Warehouse.cs](Models/Warehouse.cs), [ProductWarehouse.cs](Models/ProductWarehouse.cs), [WarehouseController.cs](Controllers/WarehouseController.cs)

- 3 physical warehouses automatically sync to 1 virtual "Total Warehouse" (WarehouseId=4)
- Each product tracks inventory quantity AND weighted average cost (WAC) per warehouse
- **Stored Procedure** [sp_CalculateWeightedAverageCost.sql](SQL/sp_CalculateWeightedAverageCost.sql) runs after every purchase to:
  - Recalculate WAC using continuous weighted average formula: `(CurrentValue + NewValue) / (CurrentQty + NewQty)`
  - Auto-sync to virtual warehouse with UPDLOCK transaction locking
  - Preserve snapshot cost for COGS in P&L reports
- When adding inventory: call SP via EF Core to update ProductWarehouse.WeightedAverageCost

### 2. Private Ledger (Debt Management)
**Files**: [PrivateLedger.cs](Models/PrivateLedger.cs), [PrivateLedgerController.cs](Controllers/PrivateLedgerController.cs)

- Track accumulated investment/debt with cumulative payments
- `Balance = TotalDebt - TotalPaid` (computed property)
- LedgerEntry records each transaction; use transaction amount filtering by date range
- Pattern: Create PrivateLedger, then append LedgerEntry records for each payment/investment

### 3. Discount Engine
**Files**: [Discount.cs](Models/Discount.cs), [DiscountService.cs](Services/DiscountService.cs)

Three discount types (DiscountType enum):
- **Fixed**: Flat amount reduction
- **Percentage**: Percent of order amount
- **Tiered**: Conditional discount ranges via DiscountTier (FromAmount/ToAmount)

**Critical Logic** in DiscountService.CalculateSupplierDiscount/CalculateCustomerDiscount:
1. Filter active discounts within date range AND meeting MinimumAmount threshold
2. Stack ALL matching discounts (order by descending DiscountValue)
3. For Tiered: query DiscountTiers matching current order amount range
4. Return cumulative discount decimal value

### 4. Financial P&L Reports
**Files**: [FinancialService.cs](Services/FinancialService.cs), [FinancialController.cs](Controllers/FinancialController.cs)

**Formula** (from ProfitLossReport):
```
GrossProfit = TotalRevenue - CostOfGoodsSold
NetProfit = GrossProfit + DiscountReceived - DiscountGiven - OperatingExpenses
```

- Revenue from SalesOrders.FinalAmount (status=Completed, within date range)
- COGS from SalesOrders.CostOfGoodsSold (uses WAC snapshot from order creation)
- Discounts and expenses aggregated similarly
- Always filter by `OrderStatus.Completed` to exclude draft/pending orders

---

## Critical Code Patterns

### Database Context Setup
[Program.cs](Program.cs) registers services:
```csharp
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Always use interface injection**: Constructor params as `IDiscountService`, not concrete class.

### EF Core Query Patterns
- Use `.Include()` for navigation properties (ProductWarehouses → Product, Warehouse)
- Use `.ToListAsync()` not `.ToList()` in Controllers
- Apply `.Where()` filtering BEFORE `.ToListAsync()` for efficiency
- Decimal properties use `[Column(TypeName = "decimal(18,2)")]` for precision

### Stored Procedure Integration
[sp_CalculateWeightedAverageCost.sql](SQL/sp_CalculateWeightedAverageCost.sql) pattern:
- Called from application layer after PurchaseOrder completion
- Uses transaction with UPDLOCK for consistency
- Returns OUTPUT parameter `@NewWeightedAverageCost`
- Must be manually created: `sqlcmd -S (localdb)\mssqllocaldb -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql`

---

## Development Workflows

### Setup & Build
```bash
cd QLBH_ThuySan
dotnet restore                                  # Restore NuGet packages
dotnet ef database update                       # Apply migrations
sqlcmd -S (localdb)\mssqllocaldb -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql
dotnet run                                      # Start on https://localhost:5001
```

### Adding New Features
1. **Model**: Create entity class in [Models/](Models/) with data annotations
2. **DbContext**: Add DbSet to [ApplicationDbContext.cs](Models/ApplicationDbContext.cs)
3. **Migration**: `dotnet ef migrations add FeatureName` then `dotnet ef database update`
4. **Service**: Add business logic service in [Services/](Services/) with interface
5. **Controller**: Create/update controller, inject service via DI
6. **View**: Add Razor view in [Views/](Views/) subdirectory

### Database Modifications
- Always create migrations: `dotnet ef migrations add <DescriptiveName>`
- Migrations stored in `Migrations/` folder (auto-generated, don't edit manually)
- For stored procedures: edit [SQL/](SQL/) files, run sqlcmd manually
- Seed data via [Migrations/</> OnModelCreating](Models/ApplicationDbContext.cs) modelBuilder.HasData()

---

## Project-Specific Conventions

### Naming & Language Mix
- **Models**: PascalCase English (Warehouse, Product, SalesOrder)
- **Database/Views**: Vietnamese field labels acceptable in UI (Tên, Địa chỉ, Tồn Kho)
- **Service Methods**: Async pattern `CalculateXxxAsync()` or `GenerateXxxAsync()`
- **Controllers**: Plural route names → Warehouse, Financial, PrivateLedger

### Model Relationships
- Use explicit [ForeignKey] attributes on navigation properties
- Virtual ICollection for one-to-many (e.g., `public virtual ICollection<InventoryTransaction>`)
- Composite unique indexes via modelBuilder.Entity().HasIndex(new { col1, col2 }).IsUnique()
- Computed properties via `=>` operator (Balance, GrossProfit, NetProfit)

### Date Handling
- Use `DateTime.Now` for server-side timestamps
- Always filter date ranges with both StartDate AND EndDate checks
- Null checks for optional EndDate (e.g., discount without expiry): `d.EndDate == null || d.EndDate >= DateTime.Now`

---

## Integration Points & Data Flows

### Purchase → Inventory → COGS Flow
1. PurchaseOrder created with supplier, details, status=Pending
2. On completion: call sp_CalculateWeightedAverageCost via EF to update ProductWarehouse WAC
3. Auto-sync to virtual warehouse
4. SalesOrder creation snapshots WAC at order time for future COGS calculation
5. P&L report uses that snapshot (not current WAC) for consistency

### Discount Calculation Flow
1. User creates SupplierDiscount or CustomerDiscount with type/value/dates
2. Optional: add DiscountTiers for tiered discounts
3. On order: call DiscountService.CalculateSupplierDiscount(supplierId, orderAmount)
4. Service returns decimal discount amount (auto-stacks if multiple apply)
5. UI displays discount, order final amount = gross - discount

### P&L Report Generation
1. FinancialService.GenerateProfitLossReportAsync(startDate, endDate)
2. Aggregates SalesOrders (status=Completed), PurchaseOrders, OperatingExpenses within range
3. Uses COGS snapshot from SalesOrder.CostOfGoodsSold (not current WAC)
4. Returns ProfitLossReport DTO with computed properties (GrossProfit, NetProfit)

---

## Known Limitations & Aspirations

### Current State (Phase 1)
✅ Complete: Warehouse, PrivateLedger, DiscountEngine, P&L Reports  
❌ Not Implemented: CRUD for Suppliers/Customers, PurchaseOrder/SalesOrder forms, Reports export, Authentication, REST API

### When Adding Features
- No authentication yet: all controllers public
- No external integrations (API calls, emails, file exports)
- UI is minimal Bootstrap 5, no JavaScript frameworks
- Models use EF Core shadow properties minimally; prefer explicit columns

---

## File Structure Reference

| Path | Purpose |
|------|---------|
| [Models/](Models/) | Entity classes + ApplicationDbContext |
| [Controllers/](Controllers/) | MVC action handlers |
| [Services/](Services/) | Business logic + interfaces |
| [Views/](Views/) | Razor templates |
| [SQL/](SQL/) | Stored procedures & scripts |
| [Properties/launchSettings.json](Properties/launchSettings.json) | Dev server config (HTTPS, ports) |
| [appsettings.json](appsettings.json) | Connection string, logging |
| [Program.cs](Program.cs) | Startup DI + middleware config |

---

## Debugging Tips

- **Null reference in view**: Check `.Include()` chains in controller query
- **WAC not updating**: Verify stored procedure was executed via sqlcmd; check ProductWarehouse.LastUpdated timestamp
- **Discount not applying**: Filter by IsActive=true, verify date range, check MinimumAmount vs order total
- **P&L mismatch**: Confirm all orders have status=Completed; WAC snapshots vs current rates
- **Migration conflicts**: Delete Migrations folder, run fresh `dotnet ef migrations add Initial` if corrupted

---

**Last Updated**: February 3, 2026  
**Version**: 1.0  
**Maintained by**: AI Coding Agents
