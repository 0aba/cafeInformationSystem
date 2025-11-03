using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace cafeInformationSystem.Models.Entities;

[Table("OrderItem")]
public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(256)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("cost", TypeName = "money")]
    public decimal Cost { get; set; }

    public virtual ICollection<OrderOrderItem> OrderOrderItems { get; set; } = new List<OrderOrderItem>();

    public string ItemInfo => $"{Name} - {Cost:C}";
}
