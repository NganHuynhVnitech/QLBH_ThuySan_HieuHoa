using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class ChiTietTon
{
    public string MaKho { get; set; } = null!;

    public string MaHang { get; set; } = null!;

    public double? SoLuongTon { get; set; }

    public decimal? GiaTriTon { get; set; }

    public virtual HangHoa MaHangNavigation { get; set; } = null!;

    public virtual Kho MaKhoNavigation { get; set; } = null!;
}
