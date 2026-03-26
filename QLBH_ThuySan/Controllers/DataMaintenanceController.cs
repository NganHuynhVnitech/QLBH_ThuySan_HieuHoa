using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class DataMaintenanceController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> SyncPayments()
        {
            try
            {
                // 1. Sync Customers
                var customers = await _context.KhachHangs.ToListAsync();
                foreach (var kh in customers)
                {
                    // Get all Sales Slips (Debits)
                    var slips = await _context.PhieuXuats
                        .Where(p => p.IdKhachHang == kh.MaDoiTuong && p.LoaiXuat == "SALES")
                        .OrderBy(p => p.NgayXuat)
                        .ToListAsync();

                    // Get all Credits (Payments / Adjustments)
                    var credits = await _context.SoRiengKhachHangs
                        .Where(e => e.MaKhachHang == kh.MaDoiTuong && (e.LoaiGiaoDich == "THANH_TOAN" || e.LoaiGiaoDich == "CAN_TRU"))
                        .SumAsync(e => e.SoTienPhatSinh ?? 0);

                    decimal remainingCredit = credits;

                    // Reset and Distribute FIFO
                    foreach (var slip in slips)
                    {
                        decimal phaiTT = slip.SoPhaiThanhToan ?? 0;
                        if (remainingCredit <= 0)
                        {
                            slip.SoDaThanhToan = 0;
                            slip.SoChuaThanhToan = phaiTT;
                            slip.TrangThaiThanhToan = "Chưa Thanh Toán";
                        }
                        else if (remainingCredit >= phaiTT)
                        {
                            slip.SoDaThanhToan = phaiTT;
                            slip.SoChuaThanhToan = 0;
                            slip.TrangThaiThanhToan = "Đã Thanh Toán";
                            remainingCredit -= phaiTT;
                        }
                        else
                        {
                            slip.SoDaThanhToan = remainingCredit;
                            slip.SoChuaThanhToan = phaiTT - remainingCredit;
                            slip.TrangThaiThanhToan = "Thanh Toán Một Phần";
                            remainingCredit = 0;
                        }
                        _context.Update(slip);
                    }

                    // Update Customer Debt Balance
                    kh.DuNoLuyKe = slips.Sum(s => s.SoChuaThanhToan ?? 0) - remainingCredit;
                    _context.Update(kh);
                }

                // 2. Sync Suppliers
                var suppliers = await _context.NhaCungCaps.ToListAsync();
                foreach (var ncc in suppliers)
                {
                    // Get all Import Slips (Debits)
                    var slips = await _context.PhieuNhaps
                        .Where(p => p.IdNhaCungCap == ncc.MaDoiTuong)
                        .OrderBy(p => p.NgayNhap)
                        .ToListAsync();

                    // Get all Payments (Credits) - Types: CHI, NHAP
                    var credits = await _context.PhieuThuChis
                        .Where(p => p.MaDoiTuong == ncc.MaDoiTuong && (p.LoaiPhieu == "CHI" || p.LoaiPhieu == "NHAP" || p.LoaiPhieu == "Chi"))
                        .SumAsync(p => p.SoTien ?? 0);

                    decimal remainingCredit = credits;

                    // Reset and Distribute FIFO
                    foreach (var slip in slips)
                    {
                        decimal phaiTT = slip.SoPhaiThanhToan ?? 0;
                        if (remainingCredit <= 0)
                        {
                            slip.SoDaThanhToan = 0;
                            slip.SoChuaThanhToan = phaiTT;
                            slip.TrangThaiThanhToan = "Chưa Thanh Toán";
                        }
                        else if (remainingCredit >= phaiTT)
                        {
                            slip.SoDaThanhToan = phaiTT;
                            slip.SoChuaThanhToan = 0;
                            slip.TrangThaiThanhToan = "Đã Thanh Toán";
                            remainingCredit -= phaiTT;
                        }
                        else
                        {
                            slip.SoDaThanhToan = remainingCredit;
                            slip.SoChuaThanhToan = phaiTT - remainingCredit;
                            slip.TrangThaiThanhToan = "Thanh Toán Một Phần";
                            remainingCredit = 0;
                        }
                        _context.Update(slip);
                    }

                    // Update Supplier Debt Balance
                    ncc.DuNoLuyKe = slips.Sum(s => s.SoChuaThanhToan ?? 0) - remainingCredit;
                    _context.Update(ncc);
                }

                await _context.SaveChangesAsync();
                return Ok("Synchronization completed successfully.");
            }
            catch (System.Exception ex)
            {
                return BadRequest("Error during synchronization: " + ex.Message);
            }
        }
    }
}
