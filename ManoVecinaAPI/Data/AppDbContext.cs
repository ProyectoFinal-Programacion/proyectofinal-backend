using Microsoft.EntityFrameworkCore;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Gig> Gigs => Set<Gig>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USER
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // GIG
        modelBuilder.Entity<Gig>()
            .HasOne(g => g.Worker)
            .WithMany(u => u.Gigs)
            .HasForeignKey(g => g.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ORDER
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(u => u.OrdersAsClient)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Worker)
            .WithMany(u => u.OrdersAsWorker)
            .HasForeignKey(o => o.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Gig)
            .WithMany()
            .HasForeignKey(o => o.GigId)
            .OnDelete(DeleteBehavior.Cascade);

        // REVIEW
        modelBuilder.Entity<Review>()
            .HasOne(r => r.FromUser)
            .WithMany(u => u.ReviewsFrom)
            .HasForeignKey(r => r.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.ToUser)
            .WithMany(u => u.ReviewsTo)
            .HasForeignKey(r => r.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}