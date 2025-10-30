using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace cafeInformationSystem.Models.Entities
{
    public enum EmployeeRole : short
    {
        Administrator = 1,
        Chef = 2,
        Waiter = 3
    }

    [Table("Employee")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        [Column("firstname")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        [Column("lastname")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(128)]
        [Column("middlename")]
        public string? MiddleName { get; set; }

        [MaxLength(256)]
        [Column("photo")]
        public string? Photo { get; set; }

        [MaxLength(256)]
        [Column("scan_employment_contract")]
        public string? ScanEmploymentContract { get; set; }


        [Required]
        [MinLength(3)]
        [MaxLength(150)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(192)]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        public EmployeeRole Role { get; set; }

        [Required]
        [Column("work_status")]
        public bool WorkStatus { get; set; } = true;


        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
        public virtual ICollection<Shift> AdminShifts { get; set; } = new List<Shift>();
        public virtual ICollection<Order> WaiterOrders { get; set; } = new List<Order>();
        public virtual ICollection<Order> ChefOrders { get; set; } = new List<Order>();
        public virtual ICollection<Table> Tables { get; set; } = new List<Table>();

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}
