using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class BangKeChietKhau
{
    public string MaBangKe { get; set; } = null!;

    public string? MaPhieuTinh { get; set; }

    public string? MaDoiTuong { get; set; }

    public decimal? TongTien { get; set; }

    public string? TrangThai { get; set; }

    public virtual PhieuTinhChietKhau? MaPhieuTinhNavigation { get; set; }
}
