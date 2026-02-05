using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class NhaCungCap
{
    public string MaDoiTuong { get; set; } = null!;

    public string? TenDoiTuong { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public string? MaSoThue { get; set; }

    public int? SoNgayDuocNo { get; set; }

    public decimal? DuNoLuyKe { get; set; }

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
}
