using ManoVecinaAPI.Models;
using ManoVecinaAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Gig> Gigs => Set<Gig>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🔥 FIX OBLIGATORIO: guardar OrderStatus como INT
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<int>();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Gigs)
            .WithOne(g => g.Worker)
            .HasForeignKey(g => g.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.OrdersAsClient)
            .WithOne(o => o.Client)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.OrdersAsWorker)
            .WithOne(o => o.Worker)
            .HasForeignKey(o => o.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.ReviewsFrom)
            .WithOne(r => r.FromUser)
            .HasForeignKey(r => r.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.ReviewsTo)
            .WithOne(r => r.ToUser)
            .HasForeignKey(r => r.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}