# Hệ Thống Quản Lý Kinh Doanh Thủy Sản Hiệu Hoa

Hệ thống quản lý toàn diện cho hoạt động kinh doanh thủy sản với các tính năng cốt lõi:

## Tính Năng Chính

### 1. Quản Lý Kho Đa Điểm & Kho Tổng Ảo
- **3 kho vật lý** + **1 kho tổng ảo** tự động đồng bộ
- Sử dụng **Stored Procedure** tính giá vốn **Bình quân gia quyền liên hoàn** (Weighted Average Cost) ngay khi nhập hàng
- Theo dõi tồn kho theo từng kho và tổng hợp
- Tự động cập nhật giá vốn khi có giao dịch nhập hàng

### 2. Sổ Riêng - Quản Lý Công Nợ
- Quản lý công nợ đầu tư một cách chi tiết
- Ghi nợ tích lũy theo từng giao dịch
- Thanh toán cấn trừ dần theo từng đợt
- Theo dõi số dư còn lại theo thời gian thực

### 3. Discount Engine - Công Cụ Chiết Khấu Động
- **Chiết khấu cố định** (Fixed Discount): Giảm giá trực tiếp
- **Chiết khấu phần trăm** (Percentage Discount): Tính theo % giá trị đơn hàng
- **Chiết khấu bậc thang** (Tiered Discount): Chiết khấu tăng theo ngưỡng giá trị
- Áp dụng cho cả **Nhà cung cấp** và **Khách hàng**
- Tự động tính toán dựa trên dữ liệu giao dịch

### 4. Báo Cáo Tài Chính P&L
Báo cáo Lãi/Lỗ (Profit & Loss) tính toán theo công thức:

```
Lãi Gộp = Doanh Thu - Giá Vốn Hàng Bán (COGS)
Lãi Ròng = Lãi Gộp + Thu Chiết Khấu - Chi Chiết Khấu - Chi Phí Vận Hành
```

- Snapshot giá vốn theo từng đơn hàng
- Tổng hợp thu/chi chiết khấu từ giao dịch
- Theo dõi chi phí vận hành
- Báo cáo theo kỳ tùy chỉnh

## Công Nghệ

- **Framework**: ASP.NET Core MVC 10.0
- **ORM**: Entity Framework Core 10.0
- **Database**: SQL Server (LocalDB hoặc SQL Server)
- **Frontend**: Bootstrap 5, Razor Views
- **Backend**: C# .NET 10

## Cài Đặt

### Yêu Cầu
- .NET SDK 10.0 hoặc cao hơn
- SQL Server 2019+ hoặc SQL Server LocalDB
- Visual Studio 2022 hoặc VS Code

### Các Bước Cài Đặt

1. **Clone repository**
```bash
git clone https://github.com/NganHuynhVnitech/QLBH_ThuySan_HieuHoa.git
cd QLBH_ThuySan_HieuHoa/QLBH_ThuySan
```

2. **Cấu hình kết nối Database**

Mở file `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QLBH_ThuySan;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Hoặc nếu dùng SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=QLBH_ThuySan;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

3. **Cài đặt EF Core Tools** (nếu chưa có)
```bash
dotnet tool install --global dotnet-ef
```

4. **Tạo Database và Migration**
```bash
# Tạo migration đầu tiên
dotnet ef migrations add InitialCreate

# Áp dụng migration để tạo database
dotnet ef database update
```

5. **Tạo Stored Procedure**

Sau khi database được tạo, chạy script SQL để tạo stored procedure:

```bash
# Sử dụng SQL Server Management Studio hoặc Azure Data Studio
# Chạy file: SQL/sp_CalculateWeightedAverageCost.sql
```

Hoặc dùng command line:
```bash
sqlcmd -S (localdb)\mssqllocaldb -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql
```

6. **Chạy ứng dụng**
```bash
dotnet run
```

Truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## Cấu Trúc Dự Án

```
QLBH_ThuySan/
├── Controllers/          # MVC Controllers
│   ├── WarehouseController.cs
│   ├── PrivateLedgerController.cs
│   └── FinancialController.cs
├── Models/              # Data Models
│   ├── ApplicationDbContext.cs
│   ├── Warehouse.cs
│   ├── Product.cs
│   ├── PurchaseOrder.cs
│   ├── SalesOrder.cs
│   ├── PrivateLedger.cs
│   ├── Discount.cs
│   └── OperatingExpense.cs
├── Services/            # Business Logic
│   ├── DiscountService.cs
│   ├── InventoryService.cs
│   └── FinancialService.cs
├── Views/               # Razor Views
│   ├── Warehouse/
│   ├── PrivateLedger/
│   ├── Financial/
│   └── Home/
├── SQL/                 # SQL Scripts
│   └── sp_CalculateWeightedAverageCost.sql
└── wwwroot/            # Static files
```

## Database Schema

### Bảng Chính

1. **Warehouses** - Quản lý kho
   - 3 kho vật lý + 1 kho tổng ảo

2. **Products** - Sản phẩm thủy sản

3. **ProductWarehouses** - Tồn kho theo kho
   - Lưu số lượng và giá vốn bình quân

4. **InventoryTransactions** - Giao dịch nhập/xuất kho

5. **PurchaseOrders/PurchaseOrderDetails** - Đơn mua hàng

6. **SalesOrders/SalesOrderDetails** - Đơn bán hàng

7. **PrivateLedgers/LedgerEntries** - Sổ riêng và các giao dịch

8. **SupplierDiscounts/CustomerDiscounts** - Chiết khấu NCC/Khách hàng

9. **DiscountTiers** - Bậc thang chiết khấu

10. **OperatingExpenses** - Chi phí vận hành

## Stored Procedure

### sp_CalculateWeightedAverageCost

Stored procedure tự động tính giá vốn bình quân gia quyền khi nhập hàng:

**Công thức:**
```
WAC_mới = (Giá_trị_tồn_hiện_tại + Giá_trị_nhập_mới) / (Số_lượng_hiện_tại + Số_lượng_nhập_mới)
```

**Tính năng:**
- Tính giá vốn cho kho vật lý
- Tự động đồng bộ lên kho tổng ảo
- Transaction-safe với UPDLOCK
- Error handling hoàn chỉnh

## Sử Dụng

### 1. Quản Lý Kho
- Truy cập menu "Quản Lý Kho"
- Xem danh sách 4 kho (3 vật lý + 1 ảo)
- Click "Xem Tồn Kho" để xem chi tiết tồn kho từng sản phẩm
- Giá vốn bình quân được tự động cập nhật

### 2. Sổ Riêng
- Truy cập menu "Sổ Riêng"
- Tạo sổ mới cho từng khoản đầu tư/công nợ
- Thêm giao dịch ghi nợ hoặc thanh toán
- Theo dõi số dư còn lại

### 3. Báo Cáo Tài Chính
- Truy cập menu "Báo Cáo Tài Chính"
- Chọn kỳ báo cáo (từ ngày - đến ngày)
- Xem báo cáo P&L chi tiết
- In báo cáo nếu cần

## Dữ Liệu Mẫu

Hệ thống tự động tạo dữ liệu mẫu khi khởi tạo database:

- 4 kho (3 vật lý + 1 ảo)
- 3 sản phẩm thủy sản (Tôm sú, Cá hồi, Mực ống)

## Phát Triển Tiếp

### Tính năng đang phát triển:
- [ ] Giao diện quản lý Nhà cung cấp
- [ ] Giao diện quản lý Khách hàng
- [ ] Tạo đơn mua hàng (Purchase Order)
- [ ] Tạo đơn bán hàng (Sales Order)
- [ ] Quản lý chiết khấu chi tiết
- [ ] Dashboard tổng quan
- [ ] Export báo cáo Excel/PDF
- [ ] Authentication & Authorization

### Cải tiến:
- [ ] API endpoints cho mobile app
- [ ] Real-time notifications
- [ ] Advanced reporting
- [ ] Inventory forecasting

## Đóng Góp

Mọi đóng góp đều được hoan nghênh! Vui lòng:
1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## License

Dự án này được phát triển cho mục đích học tập và thương mại.

## Liên Hệ

- GitHub: [@NganHuynhVnitech](https://github.com/NganHuynhVnitech)
- Project Link: [https://github.com/NganHuynhVnitech/QLBH_ThuySan_HieuHoa](https://github.com/NganHuynhVnitech/QLBH_ThuySan_HieuHoa)
