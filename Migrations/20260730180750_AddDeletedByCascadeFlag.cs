using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drivious.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByCascadeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "VehicleDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "VehicleAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Maintenances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Insurances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Incomes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "FuelLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Expenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedByCascade",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "VehicleDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "VehicleAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "FuelLogs");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedByCascade",
                table: "Drivers");
        }
    }
}
