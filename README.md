# QLBH_ThuySan_HieuHoa

Hệ thống quản lý kinh doanh Thủy sản Hiệu Hoa. Tính năng cốt lõi: 

## Tính Năng Chính

1. **Kho đa điểm & Kho tổng ảo**: 3 kho vật lý tự động đồng bộ về 1 Kho Tổng Ảo. Tự động tính giá vốn Bình quân gia quyền liên hoàn ngay khi nhập hàng sử dụng Stored Procedure.

2. **Sổ riêng**: Quản lý công nợ đầu tư, cho phép ghi nợ tích lũy và thanh toán cấn trừ dần với theo dõi số dư thời gian thực.

3. **Discount Engine**: Tính chiết khấu động phức tạp (Bậc thang/Cố định/Phần trăm) cho NCC & Khách hàng dựa trên dữ liệu giao dịch.

4. **Tài chính**: Báo cáo P&L thực tế = Doanh thu - Giá vốn snapshot + Thu chiết khấu - Chi chiết khấu - CP vận hành.

## Công Nghệ

- **ASP.NET Core MVC 10.0**
- **Entity Framework Core 10.0**
- **SQL Server**
- **Bootstrap 5**

## Cài Đặt Nhanh

```bash
# Clone repository
git clone https://github.com/NganHuynhVnitech/QLBH_ThuySan_HieuHoa.git
cd QLBH_ThuySan_HieuHoa

# Cài đặt EF Core Tools (nếu chưa có)
dotnet tool install --global dotnet-ef

# Tạo database
cd QLBH_ThuySan
dotnet ef migrations add InitialCreate
dotnet ef database update

# Chạy stored procedure
sqlcmd -S (localdb)\mssqllocaldb -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql

# Chạy ứng dụng
dotnet run
```

Truy cập: `https://localhost:5001`

## Tài Liệu Chi Tiết

Xem file [QLBH_ThuySan/README.md](QLBH_ThuySan/README.md) để biết thêm chi tiết về:
- Hướng dẫn cài đặt đầy đủ
- Cấu trúc dự án
- Database schema
- Cách sử dụng từng tính năng
- API và Services

## Cấu Trúc Dự Án

```
QLBH_ThuySan_HieuHoa/
├── QLBH_ThuySan/          # Main ASP.NET MVC Project
│   ├── Controllers/       # MVC Controllers
│   ├── Models/           # Entity Models & DbContext
│   ├── Services/         # Business Logic Services
│   ├── Views/            # Razor Views
│   ├── SQL/              # SQL Scripts & Stored Procedures
│   └── wwwroot/          # Static files (CSS, JS, libraries)
└── README.md             # This file
```

## Các Tính Năng Đã Triển Khai

### ✅ Core Features
- [x] 3 kho vật lý + 1 kho tổng ảo
- [x] Stored Procedure tính giá vốn bình quân gia quyền
- [x] Quản lý Sổ Riêng (Private Ledger)
- [x] Discount Engine (Fixed, Percentage, Tiered)
- [x] Báo cáo P&L (Profit & Loss)

### ✅ Database & Models
- [x] Entity Framework Core với SQL Server
- [x] Models: Warehouse, Product, Supplier, Customer
- [x] Purchase Order & Sales Order
- [x] Discount models (Supplier & Customer)
- [x] Operating Expenses

### ✅ Services
- [x] DiscountService - Tính chiết khấu động
- [x] InventoryService - Quản lý tồn kho và giá vốn
- [x] FinancialService - Báo cáo tài chính

### ✅ UI/UX
- [x] Bootstrap 5 responsive design
- [x] Views cho Warehouse, Private Ledger, Financial
- [x] Vietnamese language support
- [x] Dashboard với các card tính năng

## Screenshots

### Trang Chủ
Hiển thị 4 module chính: Quản Lý Kho, Sổ Riêng, Discount Engine, và Báo Cáo Tài Chính.

### Quản Lý Kho
- Danh sách 4 kho (3 vật lý + 1 ảo)
- Xem tồn kho chi tiết từng kho
- Hiển thị giá vốn bình quân gia quyền

### Sổ Riêng
- Tạo và quản lý sổ công nợ
- Thêm giao dịch ghi nợ/thanh toán
- Theo dõi số dư còn lại

### Báo Cáo P&L
- Chọn kỳ báo cáo
- Hiển thị đầy đủ: Revenue, COGS, Discounts, Operating Expenses
- Tính Gross Profit và Net Profit

## Roadmap

### Đang phát triển
- [ ] CRUD cho Suppliers & Customers
- [ ] Purchase Order management
- [ ] Sales Order management
- [ ] Dashboard với charts
- [ ] Export reports (Excel/PDF)

### Tương lai
- [ ] Authentication & Authorization
- [ ] Role-based access control
- [ ] REST API for mobile apps
- [ ] Real-time notifications
- [ ] Advanced analytics

## Đóng Góp

Mọi đóng góp đều được hoan nghênh! Vui lòng tạo Pull Request hoặc mở Issue.

## License

Dự án phát triển cho mục đích học tập và thương mại.

## Liên Hệ

- GitHub: [@NganHuynhVnitech](https://github.com/NganHuynhVnitech)
- Project: [QLBH_ThuySan_HieuHoa](https://github.com/NganHuynhVnitech/QLBH_ThuySan_HieuHoa)

