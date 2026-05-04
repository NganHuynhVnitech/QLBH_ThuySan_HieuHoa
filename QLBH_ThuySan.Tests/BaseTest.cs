using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using QLBH_ThuySan.Models;
using System.Security.Claims;

namespace QLBH_ThuySan.Tests;

public abstract class BaseTest : IDisposable
{
    protected readonly ApplicationDbContext _context;

    protected BaseTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        
        SeedMasterData();
    }

    private void SeedMasterData()
    {
        // 1. NCC
        var ncc = new NhaCungCap { MaDoiTuong = "NCC001", TenDoiTuong = "Công ty Hải Sản A", DuNoLuyKe = 0 };
        _context.NhaCungCaps.Add(ncc);

        // 2. Khách hàng
        var kh = new KhachHang { MaDoiTuong = "KH001", TenDoiTuong = "Anh Bảy Nuôi Tôm", DuNoLuyKe = 0 };
        _context.KhachHangs.Add(kh);

        // 3. Đại lý
        var dl1 = new DaiLy { MaDaiLy = "DL001", TenDaiLy = "Đại Lý Miền Tây", IsDisabled = false };
        var dl2 = new DaiLy { MaDaiLy = "DL002", TenDaiLy = "Đại Lý Cà Mau", IsDisabled = false };
        _context.DaiLys.AddRange(dl1, dl2);

        // 4. Kho
        var kho1 = new Kho { MaKho = "K001", TenKho = "Kho Chính", LoaiKho = "VAT_LY", MaDaiLyPhuTrach = "DL001" };
        var khoTong = new Kho { MaKho = "KHO_TONG_AO", TenKho = "Kho Tổng Hệ Thống", LoaiKho = "TONG_AO" };
        _context.Khos.AddRange(kho1, khoTong);

        // 5. Sản phẩm
        var sp1 = new HangHoa { MaHang = "SP001", TenHang = "Thức ăn tôm mẫu A", DonViTinh = "Bao", GiaVonHienTai = 500000, GiaBanHienTai = 550000 };
        var sp2 = new HangHoa { MaHang = "SP002", TenHang = "Thuốc dưỡng tôm", DonViTinh = "Chai", GiaVonHienTai = 150000, GiaBanHienTai = 200000 };
        _context.HangHoas.AddRange(sp1, sp2);

        _context.SaveChanges();
    }

    protected void SetupController(Controller controller)
    {
        var httpContext = new DefaultHttpContext();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim("QuyenNguoiDung", "1")
        }, "TestAuth"));
        httpContext.User = user;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var tempDataMock = new Mock<ITempDataDictionary>();
        controller.TempData = tempDataMock.Object;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
