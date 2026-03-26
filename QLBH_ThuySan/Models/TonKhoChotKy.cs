using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class TonKhoChotKy
{
    public int Id { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    public string MaKho { get; set; } = null!;

    public string MaHang { get; set; } = null!;

    public double? SoLuongTonDau { get; set; }

    public double? SoLuongNhap { get; set; }

    public double? SoLuongXuat { get; set; }

    public double? SoLuongTonCuoi { get; set; }

    public decimal? GiaTriTonCuoi { get; set; }

    public virtual HangHoa MaHangNavigation { get; set; } = null!;

    public virtual Kho MaKhoNavigation { get; set; } = null!;
}
