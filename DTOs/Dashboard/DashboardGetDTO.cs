namespace Drivious.DTOs.Dashboard
{
    public class DashboardGetDTO
    {
        public int TotalVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int ActiveVehicles { get; set; }
        public int ActiveDrivers { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public int TotalNotifications { get; set; }

        public decimal Profit { get; set; }

        /// <summary>Notifications nobody has opened yet.</summary>
        public int UnreadNotifications { get; set; }

        /// <summary>Vehicles currently out with a driver.</summary>
        public int AssignedVehicles { get; set; }

        public decimal TotalFuelCost { get; set; }
        public decimal TotalMaintenanceCost { get; set; }

        /// <summary>What is about to run out, so the fleet can act before it does.</summary>
        public UpcomingGetDTO Upcoming { get; set; } = new();

        /// <summary>Income and expense per month, oldest first.</summary>
        public List<MonthlyTotalGetDTO> MonthlyTotals { get; set; } = new();

        /// <summary>The vehicles that cost the most, highest first.</summary>
        public List<VehicleCostGetDTO> TopSpendingVehicles { get; set; } = new();
    }

    public class UpcomingGetDTO
    {
        /// <summary>How many days ahead the counts below look.</summary>
        public int WithinDays { get; set; }

        public int ExpiringInsurances { get; set; }
        public int ExpiringLicenses { get; set; }
        public int DueMaintenances { get; set; }
        public int ExpiringDocuments { get; set; }

        /// <summary>Dates that have already passed and still need attention.</summary>
        public int OverdueTotal { get; set; }
    }

    public class MonthlyTotalGetDTO
    {
        public int Year { get; set; }

        public int Month { get; set; }

        /// <summary>"2026-08", ready to use as a chart label.</summary>
        public string Label { get; set; } = null!;

        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Profit { get; set; }
    }

    public class VehicleCostGetDTO
    {
        public Guid VehicleId { get; set; }

        public string PlateNumber { get; set; } = null!;

        public string VehicleName { get; set; } = null!;

        /// <summary>Expenses, fuel and service added together.</summary>
        public decimal TotalCost { get; set; }
    }
}
