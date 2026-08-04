using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CityId",
                table: "Projects",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Cities_CityId",
                table: "Projects",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Cities_CityId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CityId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Projects");
        }
    }
}
