using Microsoft.EntityFrameworkCore;
using cafeInformationSystem.Models.Entities;
using System.Collections.Generic;

namespace cafeInformationSystem.Models.Data;

public class ApplicationDbContext : DbContext
{
    public static ApplicationDbContext? DatabaseContext { get; private set; }

    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
        }
    }

    public DbSet<Employee> Employee { get; set; }
    public DbSet<Shift> Shift { get; set; }
    public DbSet<Table> Table { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }
    public DbSet<CashReceiptOrder> CashReceiptOrder { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasMany(e => e.Shifts)
                .WithMany(s => s.Employees)
                .UsingEntity<Dictionary<string, object>>(
                    "Employee_Shifts",
                    j => j.HasOne<Shift>()
                    .WithMany()
                    .HasForeignKey("shift_fk")
                    .OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey("employee_fk")
                    .OnDelete(DeleteBehavior.Restrict),
                    j => j.ToTable("Employee_Shifts")
                );

            entity.HasMany(e => e.AdminShifts)
                .WithOne(s => s.AdminAppointed)
                .HasForeignKey(s => s.AdminAppointedId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.WaiterOrders)
                .WithOne(s => s.Waiter)
                .HasForeignKey(s => s.WaiterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.ChefOrders)
                .WithOne(s => s.Chef)
                .HasForeignKey(s => s.ChefId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Tables)
                .WithOne(s => s.WaiterService)
                .HasForeignKey(s => s.WaiterServiceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasIndex(e => e.ShiftCode).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("shift_time_ck", "time_start < time_end"));
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasIndex(t => t.TableCode).IsUnique();

            entity.HasMany(e => e.Orders)
                .WithOne(s => s.Table)
                .HasForeignKey(s => s.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.OrderCode).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("order_closed_at_ck", "closed_at IS NULL OR created_at < closed_at"));
            entity.ToTable(t => t.HasCheckConstraint("order_total_cost_ck", "total_cost > 0::money"));
            entity.ToTable(t => t.HasCheckConstraint("order_amount_clients_ck", "amount_clients > 0"));
        });

        modelBuilder.Entity<OrderOrderItem>(entity =>
        {
            entity.HasKey(ooi => ooi.Id);
            entity.HasIndex(ooi => new { ooi.OrderId, ooi.OrderItemId }).IsUnique();

            entity.HasOne(ooi => ooi.ForOrder)
                .WithMany(o => o.OrderOrderItems)
                .HasForeignKey(ooi => ooi.OrderId);
            
            entity.ToTable(t => t.HasCheckConstraint("orderorderitem_amount_items_ck", "amount_items > 0"));
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(oi => oi.Name).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("order_item_cost_ck", "cost > 0::money"));
        });

        modelBuilder.Entity<CashReceiptOrder>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("cash_receipt_order_payment_amount_ck", "payment_amount > 0::money"));
        });
    }
}
