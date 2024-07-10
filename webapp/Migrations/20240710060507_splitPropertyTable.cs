using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapp.Migrations
{
    /// <inheritdoc />
    public partial class splitPropertyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BuildingAge",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "GDPRConsent",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Garage",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "OtherFeatures",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Rooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Properties");

            migrationBuilder.CreateTable(
                name: "PropertyAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyAddresses_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BuildingAge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Garage = table.Column<int>(type: "int", nullable: false),
                    Rooms = table.Column<int>(type: "int", nullable: false),
                    OtherFeatures = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDetails_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAddresses_PropertyId",
                table: "PropertyAddresses",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDetails_PropertyId",
                table: "PropertyDetails",
                column: "PropertyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyAddresses");

            migrationBuilder.DropTable(
                name: "PropertyDetails");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuildingAge",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Properties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Properties",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "GDPRConsent",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Garage",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtherFeatures",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rooms",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
