using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShippingHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressDescription",
                table: "Shipments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CashOnDelivery",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Shipments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Shipments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressDescription",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CashOnDelivery",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Shipments");
        }
    }
}
