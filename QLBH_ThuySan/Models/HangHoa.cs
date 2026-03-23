using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class HangHoa
{
    public string MaHang { get; set; } = null!;

    public string TenHang { get; set; } = null!;

    public string? DonViTinh { get; set; }

    public string? QuyCach { get; set; }

    public decimal? GiaVonHienTai { get; set; }

    public decimal? GiaBanHienTai { get; set; }
    public bool IsDisabled { get; set; } = false;

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    public virtual ICollection<ChiTietTon> ChiTietTons { get; set; } = new List<ChiTietTon>();

    public virtual ICollection<DonViTinh> DonViTinhs { get; set; } = new List<DonViTinh>();
}
