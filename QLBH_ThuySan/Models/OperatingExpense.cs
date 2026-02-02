using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    public class OperatingExpense
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        [StringLength(50)]
        public string ExpenseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ExpenseName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
