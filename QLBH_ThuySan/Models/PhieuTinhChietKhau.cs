using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class PhieuTinhChietKhau
{
    public string MaPhieuTinh { get; set; } = null!;

    public string? LoaiDoiTuong { get; set; }

    public DateOnly? TuNgay { get; set; }

    public DateOnly? DenNgay { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<BangKeChietKhau> BangKeChietKhaus { get; set; } = new List<BangKeChietKhau>();
}
