using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cafeInformationSystem.Models.Entities;

[Table("Order_OrderItem")]
public class OrderOrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [ForeignKey("order_fk")]
    public long OrderId { get; set; }

    [Required]
    [ForeignKey("order_item_fk")]
    public long OrderItemId { get; set; }

    [Required]
    [Column("amount_items")]
    public short AmountItems { get; set; }

    public virtual Order ForOrder { get; set; } = null!;
    public virtual OrderItem СertainOrderItem { get; set; } = null!;
}
