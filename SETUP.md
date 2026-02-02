# SETUP GUIDE - Hướng Dẫn Cài Đặt

Hướng dẫn chi tiết cài đặt và chạy hệ thống QLBH Thủy Sản Hiệu Hoa.

## Yêu Cầu Hệ Thống

### Phần Mềm Bắt Buộc
- **.NET SDK 10.0** hoặc cao hơn
- **SQL Server 2019+** hoặc **SQL Server LocalDB**
- **Visual Studio 2022** (khuyến nghị) hoặc **VS Code**

### Kiểm Tra Version
```bash
# Kiểm tra .NET SDK
dotnet --version
# Kết quả mong đợi: 10.0.x hoặc cao hơn

# Kiểm tra SQL Server LocalDB
sqllocaldb info
# Kết quả mong đợi: Danh sách các instances
```

## Bước 1: Clone Repository

```bash
git clone https://github.com/NganHuynhVnitech/QLBH_ThuySan_HieuHoa.git
cd QLBH_ThuySan_HieuHoa
```

## Bước 2: Cài Đặt Dependencies

```bash
cd QLBH_ThuySan
dotnet restore
```

## Bước 3: Cấu Hình Database

### Option A: Sử dụng SQL Server LocalDB (Khuyến nghị cho Development)

File `appsettings.json` đã được cấu hình mặc định:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QLBH_ThuySan;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Option B: Sử dụng SQL Server Instance

Sửa file `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QLBH_ThuySan;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

Thay thế:
- `YOUR_SERVER_NAME`: Tên SQL Server (vd: `localhost` hoặc `.\SQLEXPRESS`)
- `YOUR_USERNAME`: Username SQL Server
- `YOUR_PASSWORD`: Password SQL Server

## Bước 4: Cài Đặt EF Core Tools

```bash
# Cài đặt global tool (chỉ cần làm 1 lần)
dotnet tool install --global dotnet-ef

# Kiểm tra version
dotnet ef --version
```

## Bước 5: Tạo Database với Migrations

```bash
# Tạo migration đầu tiên
dotnet ef migrations add InitialCreate

# Áp dụng migration để tạo database và tables
dotnet ef database update
```

**Kết quả mong đợi:**
- Database `QLBH_ThuySan` được tạo
- Tất cả tables được tạo
- Seed data được insert (4 warehouses, 3 products)

## Bước 6: Tạo Stored Procedure

### Option A: Sử dụng SQLCMD (Command Line)

```bash
# Với LocalDB
sqlcmd -S "(localdb)\mssqllocaldb" -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql

# Với SQL Server Instance
sqlcmd -S YOUR_SERVER_NAME -U YOUR_USERNAME -P YOUR_PASSWORD -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql
```

### Option B: Sử dụng SQL Server Management Studio (SSMS)

1. Mở SSMS và connect tới SQL Server
2. Mở file `SQL/sp_CalculateWeightedAverageCost.sql`
3. Chọn database `QLBH_ThuySan`
4. Execute script (F5)

### Option C: Sử dụng Azure Data Studio

1. Mở Azure Data Studio và connect tới SQL Server
2. Click "New Query"
3. Copy nội dung file `SQL/sp_CalculateWeightedAverageCost.sql`
4. Paste và Run (F5)

## Bước 7: Chạy Ứng Dụng

```bash
# Từ thư mục QLBH_ThuySan
dotnet run
```

**Kết quả mong đợi:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

## Bước 8: Truy Cập Ứng Dụng

Mở browser và truy cập:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Kiểm Tra Cài Đặt

### 1. Kiểm tra Kho
- Click "Quản Lý Kho" trên trang chủ
- Xác nhận có 4 kho: 3 vật lý + 1 ảo
- Click "Xem Tồn Kho" cho Kho Tổng Ảo
- Danh sách sẽ trống vì chưa có transaction

### 2. Kiểm tra Sổ Riêng
- Click "Sổ Riêng" trên trang chủ
- Click "Tạo Sổ Mới"
- Nhập thông tin và tạo sổ
- Thêm giao dịch để test

### 3. Kiểm tra Báo Cáo P&L
- Click "Báo Cáo Tài Chính"
- Chọn từ ngày đến ngày
- Click "Tạo Báo Cáo"
- Báo cáo sẽ hiển thị (giá trị = 0 vì chưa có dữ liệu)

## Troubleshooting

### Lỗi: "A connection was successfully established..."

**Nguyên nhân:** Không kết nối được SQL Server

**Giải pháp:**
1. Kiểm tra SQL Server đang chạy:
   ```bash
   # Với LocalDB
   sqllocaldb start mssqllocaldb
   ```
2. Kiểm tra connection string trong `appsettings.json`
3. Với SQL Server Instance, đảm bảo TCP/IP được enable

### Lỗi: "The term 'dotnet-ef' is not recognized..."

**Nguyên nhân:** EF Core Tools chưa được cài đặt

**Giải pháp:**
```bash
dotnet tool install --global dotnet-ef
# Restart terminal sau khi cài
```

### Lỗi: "There is already an object named..."

**Nguyên nhân:** Database đã tồn tại

**Giải pháp:**
```bash
# Option 1: Drop database và tạo lại
dotnet ef database drop
dotnet ef database update

# Option 2: Xóa database thủ công trong SSMS
```

### Lỗi: Build Failed

**Giải pháp:**
```bash
# Clean và rebuild
dotnet clean
dotnet restore
dotnet build
```

### Port đã được sử dụng

**Giải pháp:**
Sửa file `Properties/launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:5051;http://localhost:5050"
}
```

## Development với Visual Studio

### Mở Project
1. Double-click file `QLBH_ThuySan_HieuHoa.sln`
2. Hoặc: File → Open → Project/Solution → chọn file .sln

### Chạy Migration
1. Tools → NuGet Package Manager → Package Manager Console
2. Chạy commands:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```

### Debug
1. Đặt breakpoint (F9)
2. Press F5 để start debugging
3. Ứng dụng sẽ mở trong browser

## Development với VS Code

### Extensions Cần Thiết
- C# Dev Kit
- SQL Server (mssql)

### Tasks
File `.vscode/tasks.json` đã có sẵn các task:
- Build: `Ctrl+Shift+B`
- Run: `F5`

## Database Management

### Xem Database
```bash
# List databases
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases"

# Connect và query
sqlcmd -S "(localdb)\mssqllocaldb" -d QLBH_ThuySan
```

### Backup Database
```sql
BACKUP DATABASE QLBH_ThuySan 
TO DISK = 'C:\Backup\QLBH_ThuySan.bak'
```

### Restore Database
```sql
RESTORE DATABASE QLBH_ThuySan 
FROM DISK = 'C:\Backup\QLBH_ThuySan.bak'
```

## Next Steps

Sau khi setup thành công:

1. **Tạo dữ liệu test**: Thêm suppliers, customers, products
2. **Test Stored Procedure**: Tạo purchase order và kiểm tra giá vốn
3. **Test Discount Engine**: Tạo discount rules và test calculation
4. **Explore Code**: Đọc code trong Controllers, Services, Models

## Support

Nếu gặp vấn đề:
1. Kiểm tra phần Troubleshooting ở trên
2. Mở Issue trên GitHub
3. Liên hệ qua GitHub Profile

## Summary Commands

```bash
# Quick Setup (sau khi clone)
cd QLBH_ThuySan
dotnet restore
dotnet ef database update
sqlcmd -S "(localdb)\mssqllocaldb" -d QLBH_ThuySan -i SQL/sp_CalculateWeightedAverageCost.sql
dotnet run

# Browse to: https://localhost:5001
```

Chúc bạn thành công! 🎉
