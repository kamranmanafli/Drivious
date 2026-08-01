using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drivious.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleDocumentExpiryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "VehicleDocuments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "VehicleDocuments");
        }
    }
}
