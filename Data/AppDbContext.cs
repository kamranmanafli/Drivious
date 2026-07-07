using Drivious.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Drivious.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

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
    }
}
