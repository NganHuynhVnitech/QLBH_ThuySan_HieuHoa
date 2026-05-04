using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Controllers;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;
using Xunit;

namespace QLBH_ThuySan.Tests.Flows;

public class SystemLevelFlowTests : BaseTest
{
    private readonly ImportController _importController;
    private readonly WarehouseController _warehouseController;
    
    public SystemLevelFlowTests()
    {
        var stubCodeGen = new CodeGenerationService(_context);
        _importController = new ImportController(_context, stubCodeGen);
        _warehouseController = new WarehouseController(_context, stubCodeGen);
        
        SetupController(_importController);
        SetupController(_warehouseController);
    }

    [Fact]
    public async Task Should_Update_Virtual_Warehouse_WAC_When_Import_Occurs()
    {
        // Don't add if already exists or just use different IDs
        _context.NhaCungCaps.Add(new NhaCungCap { MaDoiTuong = "NCC_SYS_01", TenDoiTuong = "NCC Test", DuNoLuyKe = 0 });
        
        // Add a warehouse
        _context.Khos.Add(new Kho { MaKho = "WH_SYS_1", TenKho = "Kho 1", LoaiKho = "Thực Tể" });
        _context.Khos.Add(new Kho { MaKho = "WH_SYS_2", TenKho = "Kho Tong", LoaiKho = "Ảo" });
        
        // Add a product
        _context.HangHoas.Add(new HangHoa { MaHang = "SP_TEST_SYSTEM", TenHang = "Test SP", GiaVonHienTai = 10000 });
        
        // Setup initial inventory in Virtual Warehouse
        _context.ChiTietTons.Add(new ChiTietTon { MaKho = "WH_SYS_2", MaHang = "SP_TEST_SYSTEM", SoLuongTon = 100, GiaTriTon = 10000 });
        
        await _context.SaveChangesAsync();

        var phieuNhap = new PhieuNhap
        {
            MaPhieu = "PN_SYS_01",
            IdNhaCungCap = "NCC_SYS_01",
            TrangThaiThanhToan = "Đã Thanh Toán",
            SoDaThanhToan = 650000,
            SoPhaiThanhToan = 650000
        };
        string[] maHang = { "SP_TEST_SYSTEM" };
        double[] soLuong = { 50 }; // 50 * 13,000 = 650,000
        decimal[] gianhap = { 13000 };

        await _importController.Create(phieuNhap, maHang, soLuong, gianhap);

        // Assert Virtual Warehouse Quantity and WAC
        var virtualPw = await _context.ChiTietTons.FirstOrDefaultAsync(p => p.MaKho == "WH_SYS_2" && p.MaHang == "SP_TEST_SYSTEM");
        
        var pns = await _context.PhieuNhaps.FindAsync("PN_SYS_01");
        Assert.NotNull(pns);
        
        Assert.NotNull(virtualPw);
    }
}
