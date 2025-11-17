using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace cafeInformationSystem.Models.Entities;

[Table("Shift")]
public class Shift
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Column("shift_code")]
    public string ShiftCode { get; set; } = string.Empty;

    [Required]
    [Column("time_start")]
    public DateTime TimeStart { get; set; }

    [Required]
    [Column("time_end")]
    public DateTime TimeEnd { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public bool IsCompleted => TimeEnd < DateTime.Now;
    public string IsCompletedText => IsCompleted ? "завершена" : "не завершена";
    public string ShiftInfo => $"{ShiftCode} ({TimeStart:dd.MM.yyyy HH:mm} - {TimeEnd:dd.MM.yyyy HH:mm})";
}
