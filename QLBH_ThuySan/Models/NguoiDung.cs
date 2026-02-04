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
        [Column("TenNguoiDung")]
        [StringLength(50)]
        public string TenNguoiDung { get; set; } = null!;

        [Column("TenHienThi")]
        [StringLength(50)]
        public string? TenHienThi { get; set; }

        [Column("MatKhau")]
        [StringLength(50)]
        public string? MatKhau { get; set; }

        /// <summary>
        /// Permission level: 1=Admin, 2=Manager, 3=Saler
        /// </summary>
        [Column("QuyenNguoiDung")]
        public int? QuyenNguoiDung { get; set; }
    }
}
