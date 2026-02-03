using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class Kho
{
    public string MaKho { get; set; } = null!;

    public string? TenKho { get; set; }

    public string? LoaiKho { get; set; }

    public string? MaDaiLyPhuTrach { get; set; }

    public virtual ICollection<ChiTietTon> ChiTietTons { get; set; } = new List<ChiTietTon>();

    public virtual DaiLy? MaDaiLyPhuTrachNavigation { get; set; }
}
