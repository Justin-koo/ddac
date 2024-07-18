using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapp.Migrations
{
    /// <inheritdoc />
    public partial class CreatePropertySave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.CreateTable(
				name: "PropertySave",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					PropertyId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PropertySave", x => x.Id);
					table.ForeignKey(
						name: "FK_PropertySave_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropTable(
				name: "PropertySave");
		}
    }
}
