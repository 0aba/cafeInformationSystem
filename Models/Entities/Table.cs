using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace cafeInformationSystem.Models.Entities;

[Table("Table")]
public class Table
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Column("table_code")]
    public string TableCode { get; set; } = string.Empty;

    [ForeignKey("waiter_service_fk")]
    public long? WaiterServiceId { get; set; }

    public virtual Employee? WaiterService { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public bool IsOccupied => Orders.Any(o => o.Status == OrderStatus.Accepted);
}
