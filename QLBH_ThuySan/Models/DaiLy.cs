using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class DaiLy
{
    public string MaDaiLy { get; set; } = null!;

    public string? TenDaiLy { get; set; }

    public string? LoaiDaiLy { get; set; }
    public bool IsDisabled { get; set; } = false;

    public virtual ICollection<Kho> Khos { get; set; } = new List<Kho>();

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();

    public virtual ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
}
