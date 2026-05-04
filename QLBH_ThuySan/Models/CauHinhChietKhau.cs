using System;
using System.Collections.Generic;

namespace QLBH_ThuySan.Models;

public partial class CauHinhChietKhau
{
    public int Id { get; set; }

    public string? TenCauHinh { get; set; }

    public string? LoaiQuyLuat { get; set; }

    public string? DoiTuongApDung { get; set; }

    public string? ChiTietLuat { get; set; }
}
