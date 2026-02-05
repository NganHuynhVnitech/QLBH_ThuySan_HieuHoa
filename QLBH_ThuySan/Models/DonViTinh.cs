using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    [Table("DonViTinh")]
    public partial class DonViTinh
    {
        [Key]
        public int Id { get; set; }

        public string MaHang { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TenDonVi { get; set; } = null!;

        public int TyLeQuyDoi { get; set; }

        public decimal GiaBan { get; set; }

        [StringLength(50)]
        public string? MaHangDonVi { get; set; }

        [ForeignKey("MaHang")]
        public virtual HangHoa MaHangNavigation { get; set; } = null!;
    }
}
