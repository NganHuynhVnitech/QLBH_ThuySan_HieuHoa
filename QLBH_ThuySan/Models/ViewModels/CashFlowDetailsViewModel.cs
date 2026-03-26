using System.Collections.Generic;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Models.ViewModels
{
    public class CashFlowDetailsViewModel
    {
        public PhieuThuChi MainVoucher { get; set; } = null!;
        public string? ObjectName { get; set; }
        public string? ObjectAddress { get; set; }
        public string? ObjectPhone { get; set; }

        // Linked Source Documents
        public PhieuTinhChietKhau? DiscountVoucher { get; set; }
        public PhieuNhap? ImportVoucher { get; set; }
        public PhieuXuat? ExportVoucher { get; set; }

        // Helper to determine the type for UI
        public string DisplayType => MainVoucher.LoaiPhieu ?? "N/A";
    }
}
