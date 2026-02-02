using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    public class ProductWarehouse
    {
        [Key]
        public int ProductWarehouseId { get; set; }

        public int ProductId { get; set; }
        public int WarehouseId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightedAverageCost { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}
