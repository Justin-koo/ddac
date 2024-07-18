using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapp.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePropertySaveTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PropertySave_PropertyId",
                table: "PropertySave",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertySave_Properties_PropertyId",
                table: "PropertySave",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertySave_Properties_PropertyId",
                table: "PropertySave");

            migrationBuilder.DropIndex(
                name: "IX_PropertySave_PropertyId",
                table: "PropertySave");
        }
    }
}
