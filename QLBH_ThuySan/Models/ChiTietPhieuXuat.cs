using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class ChiTietPhieuXuat
{
    public string MaPhieu { get; set; } = null!;

    public string MaHang { get; set; } = null!;

    public double? SoLuong { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiaVonTaiThoiDiem { get; set; }

    public double? ThanhTien { get; set; }

    public virtual HangHoa MaHangNavigation { get; set; } = null!;

    public virtual PhieuXuat MaPhieuNavigation { get; set; } = null!;
}
