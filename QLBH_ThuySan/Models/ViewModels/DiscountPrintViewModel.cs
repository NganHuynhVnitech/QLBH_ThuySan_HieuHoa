namespace QLBH_ThuySan.Models.ViewModels
{
    public class DiscountPrintViewModel
    {
        // Header Info
        public string MaPhieuTinh { get; set; } = "";
        public DateTime NgayTao { get; set; }
        public string LoaiDoiTuong { get; set; } = "";
        public string TenDoiTuong { get; set; } = "";
        public string MaDoiTuong { get; set; } = "";
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public decimal SoPhaiThanhToanChietKhau { get; set; }
        public string TrangThai { get; set; } = "";
        public DateTime? NgayThanhToan { get; set; }

        public List<DiscountPrintProductItem> ProductDetails { get; set; } = new();
    }

    public class DiscountPrintProductItem
    {
        public string TenHang { get; set; } = "";
        public string DonViTinh { get; set; } = "";
        public List<DiscountTransactionDetail> Transactions { get; set; } = new();
        
        // Summary for this product (stored in ticket)
        public double TongSoLuong { get; set; }
        public decimal SoPhaiThanhToanChietKhau { get; set; }
        public string QuyTacApDung { get; set; } = "";
    }

    public class DiscountTransactionDetail
    {
        public DateTime NgayGiaoDich { get; set; }
        public string MaPhieu { get; set; } = ""; // Import/Export Ticket #
        public double SoLuong { get; set; }
        public decimal DonGiaMua { get; set; } // Purchase Price
        public decimal ThanhTienMua { get; set; } // Purchase Amount
        
        // Calculated/Estimated Discount for this line
        public decimal DonGiaChietKhau { get; set; } 
        public decimal ThanhTienChietKhau { get; set; }
    }
}

