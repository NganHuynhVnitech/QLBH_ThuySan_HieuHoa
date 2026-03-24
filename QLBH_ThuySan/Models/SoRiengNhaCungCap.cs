using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class SoRiengNhaCungCap
{
    public int Id { get; set; }

    public string? MaNhaCungCap { get; set; }

    public DateTime? NgayGiaoDich { get; set; }

    public string? LoaiGiaoDich { get; set; }

    public decimal? SoTienPhatSinh { get; set; }

    public string? DienGiai { get; set; }

    public virtual NhaCungCap? MaNhaCungCapNavigation { get; set; }
}
