using CarRentalManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Data
{
    // I use this DbContext to connect both Identity and my project tables to the same database.
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Application tables (Identity tables come from IdentityDbContext).
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Rental> Rentals => Set<Rental>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // I set decimal precision for money fields so SQL Server stores them properly.
            builder.Entity<Car>()
                .Property(c => c.DailyRate)
                .HasPrecision(18, 2);

            builder.Entity<Rental>()
                .Property(r => r.TotalCost)
                .HasPrecision(18, 2);


            // I enforce unique emails so we don’t end up with duplicates.
            builder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            builder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // Relationships:
            builder.Entity<Rental>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rental>()
                .HasOne(r => r.Car)
                .WithMany()
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Rental>()
                .HasOne(r => r.ApprovedByAdmin)
                .WithMany()
                .HasForeignKey(r => r.ApprovedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
