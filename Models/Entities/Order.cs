using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace cafeInformationSystem.Models.Entities
{
    public enum OrderStatus : short
    {
        Accepted = 1,
        Paid = 2,
        Cancelled = 3
    }

    [Table("Order")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("order_code")]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [Required]
        [Column("total_cost", TypeName = "money")]
        public decimal TotalCost { get; set; }

        [Required]
        [ForeignKey("waiter_fk")]
        public long WaiterId { get; set; }

        [Required]
        [ForeignKey("table_fk")]
        public long TableId { get; set; }

        [Required]
        [ForeignKey("chef_fk")]
        public long ChefId { get; set; }

        [Required]
        [Column("status")]
        public OrderStatus Status { get; set; }

        [MaxLength(512)]
        [Column("note")]
        public string? Note { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual Employee Waiter { get; set; } = null!;
        public virtual Employee Chef { get; set; } = null!;
        public virtual Table Table { get; set; } = null!;
        public virtual ICollection<CashReceiptOrder> CashReceiptOrders { get; set; } = new List<CashReceiptOrder>();

        public bool IsActive => Status == OrderStatus.Accepted;
        public string OrderInfo => $"{OrderCode} - Стол {Table?.TableCode} - {TotalCost:C} рублей";
    }
}