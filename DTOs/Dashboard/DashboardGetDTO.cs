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
    }
}
