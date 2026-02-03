using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class KhachHang
{
    public string MaDoiTuong { get; set; } = null!;

    public string? TenDoiTuong { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public string? AoNuoi { get; set; }

    public decimal? DuNoLuyKe { get; set; }

    public virtual ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();

    public virtual ICollection<SoRiengKhachHang> SoRiengKhachHangs { get; set; } = new List<SoRiengKhachHang>();
}
