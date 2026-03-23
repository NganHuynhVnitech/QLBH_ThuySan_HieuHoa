using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class PhieuXuat
{
    public string MaPhieu { get; set; } = null!;

    public DateTime? NgayXuat { get; set; }

    public string? IdDaiLyBan { get; set; }

    public string? IdKhachHang { get; set; }

    public decimal? SoPhaiThanhToan { get; set; }

    public decimal? SoDaThanhToan { get; set; }

    public decimal? SoChuaThanhToan { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public string? TrangThaiThanhToan { get; set; }

    public string LoaiXuat { get; set; } = "SALES"; // SALES, RETURN_VENDOR, DAMAGE_LOSS, TRANSFER

    public string? IdNhaCungCap { get; set; }

    public string? MaKhoNhan { get; set; }

    public string? MaKhoXuat { get; set; }

    public string? LyDo { get; set; }

    public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    public virtual DaiLy? IdDaiLyBanNavigation { get; set; }

    public virtual KhachHang? IdKhachHangNavigation { get; set; }

    public virtual NhaCungCap? IdNhaCungCapNavigation { get; set; }

    public virtual Kho? MaKhoNhanNavigation { get; set; }

    public virtual Kho? MaKhoXuatNavigation { get; set; }
}
