using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace cafeInformationSystem.Models.Entities;

[Table("CashReceiptOrder")]
public class CashReceiptOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("payed_at")]
    public DateTime PayedAt { get; set; } = DateTime.Now;

    [Required]
    [Column("payment_amount", TypeName = "money")]
    public decimal PaymentAmount { get; set; }

    [Required]
    [ForeignKey("order_fk")]
    public long OrderId { get; set; }

    [Required]
    [Column("type_pay")]
    public bool TypePay { get; set; }

    public virtual Order Order { get; set; } = null!;

    public string PaymentTypeName => TypePay ? "Безналичные" : "Наличные";
}
