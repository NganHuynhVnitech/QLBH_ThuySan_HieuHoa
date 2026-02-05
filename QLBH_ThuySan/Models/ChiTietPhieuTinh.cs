using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    public partial class ChiTietPhieuTinh
    {
        [Key]
        public int Id { get; set; }

        public string? MaPhieuTinh { get; set; }

        public string? MaHang { get; set; }

        public double? SoLuong { get; set; }

        public decimal? SoTienChietKhau { get; set; }

        public decimal? ThanhTien { get; set; }

        [StringLength(200)]
        public string? NoiDung { get; set; }

        [ForeignKey("MaHang")]
        public virtual HangHoa? MaHangNavigation { get; set; }

        [ForeignKey("MaPhieuTinh")]
        public virtual PhieuTinhChietKhau? MaPhieuTinhNavigation { get; set; }
    }
}
