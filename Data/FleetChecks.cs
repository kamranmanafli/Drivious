using Drivious.Models;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Data
{
    /// <summary>
    /// Existence checks the services run before writing a child row. Without them a
    /// bad foreign key only surfaces as a database error, which reaches the caller
    /// as a 500 instead of a message they can act on.
    /// </summary>
    public static class FleetChecks
    {
        public static Task<bool> VehicleExistsAsync(this AppDbContext context, Guid id) =>
            context.Vehicles.AnyAsync(x => x.Id == id && !x.IsDeleted);

        public static Task<bool> DriverExistsAsync(this AppDbContext context, Guid id) =>
            context.Drivers.AnyAsync(x => x.Id == id && !x.IsDeleted);

        /// <summary>
        /// Verifies the vehicle, and the driver when one is given. Returns null when
        /// everything referenced exists, otherwise the message to return to the caller.
        /// </summary>
        public static async Task<string?> ValidateReferencesAsync(
            this AppDbContext context,
            Guid? vehicleId,
            Guid? driverId = null)
        {
            if (vehicleId.HasValue && !await context.VehicleExistsAsync(vehicleId.Value))
            {
                return "Vehicle not found.";
            }

            if (driverId.HasValue && !await context.DriverExistsAsync(driverId.Value))
            {
                return "Driver not found.";
            }

            return null;
        }

        /// <summary>
        /// Advances the odometer from a reading taken by a fuel stop or a service
        /// visit. Readings older than what is already recorded are ignored rather
        /// than rejected, so historical rows can still be backfilled.
        /// </summary>
        public static async Task AdvanceMileageAsync(
            this AppDbContext context,
            Guid vehicleId,
            int mileage)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);

            if (vehicle != null && mileage > vehicle.Mileage)
            {
                vehicle.Mileage = mileage;
                vehicle.UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Counts the rows that would be destroyed by a hard delete of this vehicle.
        /// </summary>
        public static async Task<int> CountVehicleChildrenAsync(this AppDbContext context, Guid vehicleId)
        {
            return await context.Expenses.CountAsync(x => x.VehicleId == vehicleId)
                 + await context.Incomes.CountAsync(x => x.VehicleId == vehicleId)
                 + await context.Maintenances.CountAsync(x => x.VehicleId == vehicleId)
                 + await context.Insurances.CountAsync(x => x.VehicleId == vehicleId)
                 + await context.FuelLogs.CountAsync(x => x.VehicleId == vehicleId)
                 + await context.VehicleDocuments.CountAsync(x => x.VehicleId == vehicleId)
                 + await context.VehicleAssignments.CountAsync(x => x.VehicleId == vehicleId);
        }

        public static async Task<int> CountDriverChildrenAsync(this AppDbContext context, Guid driverId)
        {
            return await context.Incomes.CountAsync(x => x.DriverId == driverId)
                 + await context.VehicleAssignments.CountAsync(x => x.DriverId == driverId);
        }

        /// <summary>
        /// Soft deletes or restores everything hanging off a vehicle, so a "deleted"
        /// vehicle does not leave its expenses and fuel logs visible in list results.
        /// </summary>
        public static async Task CascadeVehicleSoftDeleteAsync(
            this AppDbContext context,
            Guid vehicleId,
            bool isDeleted)
        {
            var stamp = isDeleted ? DateTime.UtcNow : (DateTime?)null;

            await CascadeAsync(context.Expenses.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
            await CascadeAsync(context.Incomes.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
            await CascadeAsync(context.Maintenances.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
            await CascadeAsync(context.Insurances.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
            await CascadeAsync(context.FuelLogs.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
            await CascadeAsync(context.VehicleDocuments.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
            await CascadeAsync(context.VehicleAssignments.Where(x => x.VehicleId == vehicleId), isDeleted, stamp);
        }

        public static async Task CascadeDriverSoftDeleteAsync(
            this AppDbContext context,
            Guid driverId,
            bool isDeleted)
        {
            var stamp = isDeleted ? DateTime.UtcNow : (DateTime?)null;

            await CascadeAsync(context.Incomes.Where(x => x.DriverId == driverId), isDeleted, stamp);
            await CascadeAsync(context.VehicleAssignments.Where(x => x.DriverId == driverId), isDeleted, stamp);
        }

        private static async Task CascadeAsync<T>(IQueryable<T> query, bool isDeleted, DateTime? stamp)
            where T : Models.BaseModels.BaseEntity
        {
            // Deleting takes the live rows; restoring takes back only the rows this
            // cascade removed, so a child deleted deliberately stays deleted.
            var rows = isDeleted
                ? await query.Where(x => !x.IsDeleted).ToListAsync()
                : await query.Where(x => x.IsDeleted && x.DeletedByCascade).ToListAsync();

            foreach (var row in rows)
            {
                row.IsDeleted = isDeleted;
                row.DeletedAt = stamp;
                row.DeletedByCascade = isDeleted;
            }
        }
    }
}
