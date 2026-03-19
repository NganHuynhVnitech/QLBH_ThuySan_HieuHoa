using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.ViewModels;

namespace QLBH_ThuySan.Services
{
    public class OutboundService : IOutboundService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public OutboundService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private async Task ValidateAndDeductStock(string maKho, string maHang, double soLuong, string tenKhoVatLy)
        {
            var ct = await _context.ChiTietTons.FirstOrDefaultAsync(c => c.MaKho == maKho && c.MaHang == maHang);
            if (ct == null || ct.SoLuongTon < soLuong)
            {
                throw new InvalidOperationException($"Không đủ tồn kho cho sản phẩm {maHang} tại {tenKhoVatLy}. Tồn hiện tại: {ct?.SoLuongTon ?? 0}");
            }
            ct.SoLuongTon -= soLuong;
            // GiaTriTon is moving average, we leave it proportional or recalculate:
            // Since we use moving average, removing qty means removing proportional value: average cost * removedQty
            var hh = await _context.HangHoas.FindAsync(maHang);
            decimal giaVon = hh?.GiaVonHienTai ?? 0;
            ct.GiaTriTon -= (decimal)soLuong * giaVon;
        }

        private async Task ApplyInventoryChangesAsync(OutboundBaseDto dto, string prefix, string loaiXuat, Action<PhieuXuat> modifyPhieu)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var khoVatLy = await _context.Khos.FindAsync(dto.SourceWarehouseCode);
                if (khoVatLy == null) throw new Exception("Kho xuất không tồn tại.");

                string maKhoTongAo = _config.GetValue<string>("KhoTongAo_MaKho") ?? "KHO_TONG";

                string maPhieu = prefix + "_" + DateTime.Now.ToString("ddMMyyHHmmssff");
                decimal tongTien = 0;

                var phieuXuat = new PhieuXuat
                {
                    MaPhieu = maPhieu,
                    NgayXuat = DateTime.Now,
                    LoaiXuat = loaiXuat,
                    IdDaiLyBan = khoVatLy.MaDaiLyPhuTrach,
                    TrangThaiThanhToan = "Chưa Thanh Toán"
                };
                
                modifyPhieu(phieuXuat);

                _context.PhieuXuats.Add(phieuXuat);

                foreach (var item in dto.Items)
                {
                    var hh = await _context.HangHoas.FindAsync(item.MaHang);
                    if (hh == null) throw new Exception($"Mã hàng {item.MaHang} không tồn tại.");

                    decimal giaVonHienTai = hh.GiaVonHienTai ?? 0;
                    decimal giaBan = item.GiaBan > 0 ? item.GiaBan : giaVonHienTai;
                    
                    var ctPx = new ChiTietPhieuXuat
                    {
                        MaPhieu = maPhieu,
                        MaHang = item.MaHang,
                        SoLuong = item.SoLuong,
                        GiaBan = giaBan,
                        GiaVonTaiThoiDiem = giaVonHienTai
                    };
                    _context.ChiTietPhieuXuats.Add(ctPx);

                    tongTien += (decimal)item.SoLuong * giaBan;

                    // Deduct Physical Stock
                    await ValidateAndDeductStock(dto.SourceWarehouseCode, item.MaHang, item.SoLuong, "Kho Vật Lý");

                    // For Transfers, we DO NOT deduct Virtual Stock (KHO_TONG) because it stays within the franchise.
                    // For Sales, Returns, and Damage, we DO deduct Virtual Stock because it leaves the ecosystem.
                    if (loaiXuat != "TRANSFER")
                    {
                        await ValidateAndDeductStock(maKhoTongAo, item.MaHang, item.SoLuong, "Kho Tổng (Ảo)");
                    }
                    else
                    {
                        // Transfer logic: Add to Destination Physical Warehouse
                        var transferDto = (OutboundTransferDto)dto;
                        var destCt = await _context.ChiTietTons.FirstOrDefaultAsync(c => c.MaKho == transferDto.DestinationWarehouseCode && c.MaHang == item.MaHang);
                        if (destCt == null)
                        {
                            _context.ChiTietTons.Add(new ChiTietTon { MaKho = transferDto.DestinationWarehouseCode, MaHang = item.MaHang, SoLuongTon = item.SoLuong, GiaTriTon = (decimal)item.SoLuong * giaVonHienTai });
                        }
                        else
                        {
                            destCt.SoLuongTon += item.SoLuong;
                            destCt.GiaTriTon += (decimal)item.SoLuong * giaVonHienTai;
                        }
                    }
                }

                phieuXuat.TongTien = tongTien;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CreateSalesOutboundAsync(OutboundSalesDto dto)
        {
            await ApplyInventoryChangesAsync(dto, "PX_SALE", "SALES", phieu => {
                phieu.IdKhachHang = dto.CustomerId;
                phieu.TrangThaiThanhToan = dto.PaymentStatus;

                // Create Customer Debt Ledger if Customer explicitly provided
                if (!string.IsNullOrEmpty(dto.CustomerId))
                {
                    decimal amount = dto.Items.Sum(x => (decimal)x.SoLuong * x.GiaBan);

                    _context.SoRiengKhachHangs.Add(new SoRiengKhachHang
                    {
                        MaKhachHang = dto.CustomerId,
                        NgayGiaoDich = DateTime.Now,
                        LoaiGiaoDich = "Mua Hàng",
                        SoTienPhatSinh = amount,
                        DienGiai = $"Tự động ghi nợ xuất bán hàng (POS)"
                    });

                    // Update accumulated debt if not paid immediately
                    if (dto.PaymentStatus != "Đã Thanh Toán")
                    {
                        var kh = _context.KhachHangs.Find(dto.CustomerId);
                        if (kh != null)
                        {
                            kh.DuNoLuyKe += amount;
                        }
                    }
                }
            });
            return true;
        }

        public async Task<bool> CreateReturnVendorOutboundAsync(OutboundReturnVendorDto dto)
        {
            await ApplyInventoryChangesAsync(dto, "PX_RET", "RETURN_VENDOR", phieu => {
                phieu.IdNhaCungCap = dto.VendorId;
                phieu.TrangThaiThanhToan = "Hoàn Tất";
            });
            return true;
        }

        public async Task<bool> CreateDamageOutboundAsync(OutboundDamageDto dto)
        {
            await ApplyInventoryChangesAsync(dto, "PX_DAM", "DAMAGE_LOSS", phieu => {
                phieu.LyDo = dto.Reason;
                phieu.TrangThaiThanhToan = "Hoàn Tất";
            });
            return true;
        }

        public async Task<bool> CreateTransferOutboundAsync(OutboundTransferDto dto)
        {
            await ApplyInventoryChangesAsync(dto, "PX_TRA", "TRANSFER", phieu => {
                phieu.MaKhoNhan = dto.DestinationWarehouseCode;
                phieu.TrangThaiThanhToan = "Hoàn Tất";
            });
            return true;
        }
    }
}
