using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class PhieuThuChi
{
    public string MaPhieu { get; set; } = null!;

    public string? LoaiPhieu { get; set; }

    public DateTime? NgayLap { get; set; }

    public decimal? SoTien { get; set; }

    public string? LyDo { get; set; }
}
