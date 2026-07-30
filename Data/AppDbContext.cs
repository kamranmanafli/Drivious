using Drivious.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<VehicleAssignment> VehicleAssignments { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<FuelLog> FuelLogs { get; set; }
        public DbSet<VehicleDocument> VehicleDocuments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // A driver account is optional and a driver record must not be blocked
            // from deletion by an orphaned login, so the link is set to null instead.
            builder.Entity<AppUser>()
                .HasOne(x => x.Driver)
                .WithOne(x => x.User)
                .HasForeignKey<AppUser>(x => x.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RefreshToken>()
                .HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(x => x.Token)
                .IsUnique();

            builder.Entity<RefreshToken>()
                .Property(x => x.Token)
                .HasMaxLength(128);

            builder.Entity<RefreshToken>()
                .Ignore(x => x.IsActive);

            // Lets the notification generator insert only what does not exist yet.
            builder.Entity<Notification>()
                .Property(x => x.ReferenceKey)
                .HasMaxLength(200);

            builder.Entity<Notification>()
                .HasIndex(x => x.ReferenceKey)
                .IsUnique()
                .HasFilter("[ReferenceKey] IS NOT NULL");

            // SQL Server cannot index nvarchar(max), so these two need an explicit length.
            // Lengths match the FluentValidation rules for the same fields.
            builder.Entity<Vehicle>()
                .Property(x => x.VIN)
                .HasMaxLength(17);

            builder.Entity<Vehicle>()
                .Property(x => x.PlateNumber)
                .HasMaxLength(20);

            // Filtered so a soft-deleted vehicle does not block reusing its plate or VIN.
            builder.Entity<Vehicle>()
                .HasIndex(x => x.VIN)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Entity<Vehicle>()
                .HasIndex(x => x.PlateNumber)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Without this EF silently falls back to decimal(18,2) and logs a warning
            // for every money column on startup.
            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}
