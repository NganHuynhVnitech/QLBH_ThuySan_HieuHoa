using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class SoRiengKhachHang
{
    public int Id { get; set; }

    public string? MaKhachHang { get; set; }

    public DateTime? NgayGiaoDich { get; set; }

    public string? LoaiGiaoDich { get; set; }

    public decimal? SoTienPhatSinh { get; set; }

    public string? DienGiai { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }
}
