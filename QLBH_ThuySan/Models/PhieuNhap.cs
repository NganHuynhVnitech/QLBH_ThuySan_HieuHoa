using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class PhieuNhap
{
    public string MaPhieu { get; set; } = null!;

    public DateTime? NgayNhap { get; set; }

    public string? IdDaiLyNhap { get; set; }

    public string? IdNhaCungCap { get; set; }

    public DateTime? HanThanhToan { get; set; }

    public decimal? SoPhaiThanhToan { get; set; }

    public decimal? SoDaThanhToan { get; set; }

    public decimal? SoChuaThanhToan { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public string? TrangThaiThanhToan { get; set; }
    public bool IsDisabled { get; set; } = false;

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual DaiLy? IdDaiLyNhapNavigation { get; set; }

    public virtual NhaCungCap? IdNhaCungCapNavigation { get; set; }
}
