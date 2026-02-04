using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    /// <summary>
    /// User entity - Maps to NguoiDung table in HieuHoaDB
    /// Used for authentication and authorization
    /// </summary>
    public class NguoiDung
    {
        [Key]
        [Column("maNguoiDung")]
        [StringLength(20)]
        public string MaNguoiDung { get; set; } = null!;

        [Column("tenNguoiDung")]
        [StringLength(100)]
        public string? TenNguoiDung { get; set; }

        [Column("matKhau")]
        [StringLength(100)]
        public string? MatKhau { get; set; }

        /// <summary>
        /// Permission level: 1=Admin, 2=Manager, 3=Saler
        /// </summary>
        [Column("quyenNguoiDung")]
        public int? QuyenNguoiDung { get; set; }
    }
}
