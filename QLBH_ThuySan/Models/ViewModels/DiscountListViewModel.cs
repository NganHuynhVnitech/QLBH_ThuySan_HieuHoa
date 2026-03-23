namespace QLBH_ThuySan.Models.ViewModels
{
    public class DiscountListViewModel
    {
        // Filters
        public string? SearchTerm { get; set; } // Partner Name or Code
        public string? Status { get; set; }
        public string? Type { get; set; } // NCC / KHACH
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public DateTime? PaymentFrom { get; set; }
        public DateTime? PaymentTo { get; set; }
        public string? RuleSearch { get; set; }

        public List<DiscountListItem> Items { get; set; } = new();
    }

    public class DiscountListItem
    {
        public string MaPhieuTinh { get; set; } = "";
        public DateTime NgayTao { get; set; }
        public string LoaiDoiTuong { get; set; } = "";
        public string MaDoiTuong { get; set; } = "";
        public string TenDoiTuong { get; set; } = "";
        public DateOnly? TuNgay { get; set; }
        public DateOnly? DenNgay { get; set; }
        public decimal SoPhaiThanhToan { get; set; }
        public decimal SoDaThanhToan { get; set; }
        public decimal SoChuaThanhToan { get; set; }
        public string TrangThai { get; set; } = "";
        public DateTime? NgayThanhToan { get; set; }
        public string QuyTacChietKhau { get; set; } = ""; // Summary of rules
    }
}

