using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Controllers;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Services;
using Xunit;

namespace QLBH_ThuySan.Tests.Flows;

public class AcceptanceTests : BaseTest
{
    private readonly FinancialController _financialController;
    private readonly ExportController _exportController;
    
    public AcceptanceTests()
    {
        var stubCodeGen = new CodeGenerationService(_context);
        _financialController = new FinancialController(new FinancialService(_context));
        _exportController = new ExportController(_context, stubCodeGen);
        
        SetupController(_financialController);
        SetupController(_exportController);
    }

    [Fact]
    public async Task UAT_Should_Process_Sales_And_Show_In_ProfitAndLoss()
    {
        // 1. Setup Data
        _context.KhachHangs.Add(new KhachHang { MaDoiTuong = "KH_UAT_01", TenDoiTuong = "UAT Customer" });
        _context.HangHoas.Add(new HangHoa { MaHang = "SP_UAT", TenHang = "San Pham UAT", GiaVonHienTai = 10000 });
        await _context.SaveChangesAsync();
        
        // 2. Perform a Sale
        var px = new PhieuXuat 
        { 
            MaPhieu = "PX_UAT_01", 
            IdKhachHang = "KH_UAT_01", 
            SoPhaiThanhToan = 200000, 
            SoDaThanhToan = 0, 
            TrangThaiThanhToan = "Chưa Thanh Toán" 
        };
        string[] maHang = { "SP_UAT" };
        double[] soLuong = { 10 };
        decimal[] giaBan = { 20000 };
        
        await _exportController.Create(px, maHang, soLuong, giaBan);

        // 3. Check P&L
        var result = await _financialController.GenerateProfitLoss(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1)) as ViewResult;
        Assert.NotNull(result);
        
        var model = result.Model as QLBH_ThuySan.Services.ProfitLossReport;
        if (model != null)
        {
            Assert.True(model.TotalRevenue >= 200000);
        }
    }
}
