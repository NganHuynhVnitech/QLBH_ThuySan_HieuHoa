using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Controllers;
using QLBH_ThuySan.Models;
using Xunit;

namespace QLBH_ThuySan.Tests.Flows;

public class CashFlowAndDebtTests : BaseTest
{
    private readonly ExportController _exportController;
    private readonly CashFlowController _cashFlowController;

    public CashFlowAndDebtTests()
    {
        _exportController = new ExportController(_context);
        _cashFlowController = new CashFlowController(_context);
        
        SetupController(_exportController);
        SetupController(_cashFlowController);
    }

    [Fact]
    public async Task Should_Increase_Customer_Debt_When_Sales_Slip_Created()
    {
        // Arrange
        var kh = await _context.KhachHangs.FindAsync("KH001");
        decimal initialDebt = kh!.DuNoLuyKe ?? 0;
        
        var phieuXuat = new PhieuXuat 
        { 
            MaPhieu = "PX_TEST_01", 
            IdKhachHang = "KH001", 
            TrangThaiThanhToan = "Chưa Thanh Toán" 
        };
        string[] maHang = { "SP001" };
        double[] soLuong = { 10 }; // 10 * 550,000 = 5,500,000
        decimal[] giaBan = { 550000 };

        // Act
        await _exportController.Create(phieuXuat, maHang, soLuong, giaBan);

        // Assert
        var updatedKh = await _context.KhachHangs.FindAsync("KH001");
        Assert.Equal(initialDebt + 5500000, updatedKh!.DuNoLuyKe);
        
        var ledger = await _context.SoRiengKhachHangs
            .FirstOrDefaultAsync(l => l.MaKhachHang == "KH001" && l.LoaiGiaoDich == "MUA_HANG");
        Assert.NotNull(ledger);
        Assert.Equal(5500000, ledger.SoTienPhatSinh);
    }

    [Fact]
    public async Task Should_Distribute_Payment_Using_FIFO_And_Create_Voucher()
    {
        // Arrange
        // Create an unpaid slip first
        var kh = await _context.KhachHangs.FindAsync("KH001");
        kh!.DuNoLuyKe = 10000000;
        _context.Update(kh);
        
        var px = new PhieuXuat { MaPhieu = "PX_UNPAID", IdKhachHang = "KH001", SoPhaiThanhToan = 10000000, SoChuaThanhToan = 10000000, TrangThaiThanhToan = "Chưa Thanh Toán" };
        _context.PhieuXuats.Add(px);
        await _context.SaveChangesAsync();

        // Act - Pay 4,000,000
        await _exportController.Pay("PX_UNPAID", 4000000, "Khách trả một phần");

        // Assert
        var updatedKh = await _context.KhachHangs.FindAsync("KH001");
        Assert.Equal(6000000, updatedKh!.DuNoLuyKe);
        
        var updatedPx = await _context.PhieuXuats.FindAsync("PX_UNPAID");
        Assert.Equal(4000000, updatedPx!.SoDaThanhToan);
        Assert.Equal("Thanh Toán Một Phần", updatedPx!.TrangThaiThanhToan);

        var voucher = await _context.PhieuThuChis.FirstOrDefaultAsync(v => v.MaDoiTuong == "KH001" && v.SoTien == 4000000);
        Assert.NotNull(voucher);
        Assert.Equal("THU BAN HANG", voucher.LoaiPhieu);
    }

    [Fact]
    public async Task Should_Reverse_Debt_And_Disable_Vouchers_When_Sales_Slip_Deleted()
    {
        // Arrange
        var kh = await _context.KhachHangs.FindAsync("KH001");
        kh!.DuNoLuyKe = 5000000;
        var px = new PhieuXuat { MaPhieu = "PX_TO_DELETE", IdKhachHang = "KH001", SoPhaiThanhToan = 5000000, SoChuaThanhToan = 5000000, TrangThaiThanhToan = "Chưa Thanh Toán", LoaiXuat = "SALES" };
        _context.PhieuXuats.Add(px);
        
        // Associated voucher
        var voucher = new PhieuThuChi 
        { 
            MaPhieu = "V_ASSOC", 
            LoaiPhieu = "THU", 
            SoTien = 1000000, 
            MaDoiTuong = "KH001", 
            LoaiDoiTuong = "KH", 
            LyDo = "Thu cho PX_TO_DELETE", 
            IsDisabled = false 
        };
        _context.PhieuThuChis.Add(voucher);
        await _context.SaveChangesAsync();

        // Act
        await _exportController.DeleteConfirmed("PX_TO_DELETE");

        // Assert
        var updatedKh = await _context.KhachHangs.FindAsync("KH001");
        // Initial 5M - 5M (from PX delete) + 1M (from Voucher disable) = 1M
        // Wait, if PX had 5M debt, and we delete it, debt decreases by 5M. 
        // If it had a payment of 1M, and we delete that payment voucher, debt increases by 1M.
        Assert.Equal(1000000, updatedKh!.DuNoLuyKe);

        var deletedPx = await _context.PhieuXuats.FindAsync("PX_TO_DELETE");
        Assert.True(deletedPx!.IsDisabled);

        var disabledVoucher = await _context.PhieuThuChis.FindAsync("V_ASSOC");
        Assert.True(disabledVoucher!.IsDisabled);
    }

    [Fact]
    public async Task Should_Increase_Debt_Back_When_Payment_Voucher_Deleted()
    {
        // Arrange
        // (Scenario: Customer paid 2M, debt decreased to 8M. We delete the 2M receipt, debt should go back to 10M)
        var kh = await _context.KhachHangs.FindAsync("KH001");
        kh!.DuNoLuyKe = 8000000;
        
        var voucher = new PhieuThuChi 
        { 
            MaPhieu = "PT_COLLECTION", 
            LoaiPhieu = "THU KHACH HANG", 
            SoTien = 2000000, 
            MaDoiTuong = "KH001", 
            LoaiDoiTuong = "KH",
            IsDisabled = false 
        };
        _context.PhieuThuChis.Add(voucher);
        await _context.SaveChangesAsync();

        // Act
        await _cashFlowController.DeleteConfirmed("PT_COLLECTION");

        // Assert
        var updatedKh = await _context.KhachHangs.FindAsync("KH001");
        Assert.Equal(10000000, updatedKh!.DuNoLuyKe);
        
        var disabledVoucher = await _context.PhieuThuChis.FindAsync("PT_COLLECTION");
        Assert.True(disabledVoucher!.IsDisabled);
    }
}
