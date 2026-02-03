using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class ChiTietPhieuNhap
{
    public string MaPhieu { get; set; } = null!;

    public string MaHang { get; set; } = null!;

    public double? SoLuong { get; set; }

    public decimal? DonGiaNhap { get; set; }

    public double? ThanhTien { get; set; }

    public virtual HangHoa MaHangNavigation { get; set; } = null!;

    public virtual PhieuNhap MaPhieuNavigation { get; set; } = null!;
}
