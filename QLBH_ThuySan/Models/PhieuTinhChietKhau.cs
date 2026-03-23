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

    public string? MaDoiTuong { get; set; }

    public decimal? SoPhaiThanhToan { get; set; }

    public decimal? SoDaThanhToan { get; set; }

    public decimal? SoChuaThanhToan { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public virtual ICollection<ChiTietPhieuTinh> ChiTietPhieuTinhs { get; set; } = new List<ChiTietPhieuTinh>();
}
