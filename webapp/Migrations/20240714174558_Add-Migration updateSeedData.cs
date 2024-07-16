using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace webapp.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationupdateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "FeatureID", "FeatureName", "IconClass" },
                values: new object[,]
                {
                    { 1, "Equipped Kitchen", "fa-utensils" },
                    { 2, "Gym", "fa-dumbbell" },
                    { 3, "Laundry", "fa-jug-detergent" },
                    { 4, "Media Room", "fa-chromecast" },
                    { 5, "TV Set", "fa-tv" },
                    { 6, "Back yard", "fa-canadian-maple-leaf" },
                    { 7, "Basketball court", "fa-basketball" },
                    { 8, "Front yard", "fa-seedling" },
                    { 9, "Garage Attached", "fa-square-parking" },
                    { 10, "Hot Bath", "fa-shower" },
                    { 11, "Pool", "fa-water-ladder" },
                    { 12, "Central Air", "fa-fan" },
                    { 13, "Electricity", "fa-plug" },
                    { 14, "Heating", "fa-fire" },
                    { 15, "Natural Gas", "fa-fire-flame-simple" },
                    { 16, "Ventilation", "fa-snowflake" },
                    { 17, "Water", "fa-droplet" },
                    { 18, "Chair Accessible", "fa-wheelchair" },
                    { 19, "Elevator", "fa-elevator" },
                    { 20, "Fireplace", "fa-fire-extinguisher" },
                    { 21, "Smoke detectors", "fa-smoking" },
                    { 22, "Washer and dryer", "fa-bacon" },
                    { 23, "WiFi", "fa-wifi" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "FeatureID",
                keyValue: 23);
        }
    }
}
