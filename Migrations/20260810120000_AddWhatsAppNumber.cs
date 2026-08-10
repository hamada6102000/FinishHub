using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsAppNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppNumbers", x => x.Id);
                    // Enforces the "exactly zero or one WhatsApp number" rule at the database
                    // level: the only insertable key is 1, so a second row is impossible even
                    // when records are created outside the Dashboard.
                    table.CheckConstraint("CK_WhatsAppNumbers_SingleRow", "[Id] = 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppNumbers_PhoneNumber",
                table: "WhatsAppNumbers",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppNumbers");
        }
    }
}
