using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    public enum LedgerType
    {
        Investment = 1,
        Debt = 2
    }

    public class PrivateLedger
    {
        [Key]
        public int LedgerId { get; set; }

        [Required]
        [StringLength(50)]
        public string LedgerCode { get; set; } = string.Empty;

        public LedgerType LedgerType { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance => TotalDebt - TotalPaid;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }

    public class LedgerEntry
    {
        [Key]
        public int EntryId { get; set; }

        public int LedgerId { get; set; }

        [Required]
        [StringLength(50)]
        public string EntryCode { get; set; } = string.Empty;

        public DateTime EntryDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebtAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("LedgerId")]
        public virtual PrivateLedger PrivateLedger { get; set; } = null!;
    }
}
