using Microsoft.EntityFrameworkCore;
using Ride_Hailing_API.Domain.Entities;

namespace Ride_Hailing_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<Otp> Otps { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Kyc> Kycs { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<RideStatusHistory> RidesStatusHistories { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Ride>()
            .HasOne(r => r.Passenger)
            .WithMany()
            .HasForeignKey(r => r.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);
            

        modelBuilder.Entity<Ride>()
            .HasOne(r => r.Driver)
            .WithMany()
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ride>()
            .HasIndex(r => r.Reference)
            .IsUnique();
        
        modelBuilder.Entity<Ride>()
            .Property(r => r.Status)
            .HasConversion<string>();
        
        modelBuilder.Entity<Otp>()
            .Property(o => o.Purpose)
            .HasConversion<string>();

        modelBuilder.Entity<Kyc>()
            .Property(k => k.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Kyc>()
            .HasIndex(k => k.UserId)
            .IsUnique();
        
        modelBuilder.Entity<RideStatusHistory>()
            .Property(r => r.PreviousStatus)
            .HasConversion<string>();
        
        modelBuilder.Entity<RideStatusHistory>()
            .Property(r => r.NewStatus)
            .HasConversion<string>();
        
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.DriverId)
            .IsUnique();
        
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.PlateNumber)
            .IsUnique();
    }
}