namespace Drivious.Constants
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Driver = "Driver";

        /// <summary>Roles allowed to change fleet data.</summary>
        public const string ManageFleet = Admin + "," + Manager;

        /// <summary>Roles allowed to read fleet data.</summary>
        public const string ReadFleet = Admin + "," + Manager + "," + Driver;

        public static readonly string[] All = { Admin, Manager, Driver };
    }
}
