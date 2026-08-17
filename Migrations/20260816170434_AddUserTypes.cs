using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test.Migrations
{
    /// <summary>
    /// Introduces the configurable UserTypes table and points Users at it.
    ///
    /// Existing users keep the type they already have: Users.UserType is an nvarchar column
    /// holding the enum name ("User" / "Engineer"), so the backfill below reads it directly and
    /// maps every row onto the matching seeded type. The old column is deliberately left in
    /// place — it still carries the behaviour bucket the engineer/client rules read — so this
    /// migration adds data and never destroys any.
    /// </summary>
    /// <inheritdoc />
    public partial class AddUserTypes : Migration
    {
        /// <summary>Fixed stamp for the seeded rows so the migration stays deterministic.</summary>
        private static readonly DateTime SeedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_Code",
                table: "UserTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_NameAr",
                table: "UserTypes",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_NameEn",
                table: "UserTypes",
                column: "NameEn",
                unique: true);

            // The two built-in types. Their ids intentionally match the old enum values
            // (User = 1, Engineer = 2), which is what makes the backfill below a straight map.
            migrationBuilder.InsertData(
                table: "UserTypes",
                columns: new[] { "Id", "NameAr", "NameEn", "Code", "Kind", "IsSystem", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "مستخدم", "User", "USER", "User", true, true, SeedCreatedAt },
                    { 2, "مهندس", "Engineer", "ENGINEER", "Engineer", true, true, SeedCreatedAt },
                });

            // Nullable first so existing rows survive the add, then backfilled, then tightened.
            migrationBuilder.AddColumn<int>(
                name: "UserTypeId",
                table: "Users",
                type: "int",
                nullable: true);

            // Map every existing user onto their seeded type. The IN-list also covers rows whose
            // UserType was written as a raw number rather than an enum name; anything else
            // (including an empty or unrecognised value) falls back to the User type, which is
            // how the application already treats undefined values today.
            migrationBuilder.Sql(@"
                UPDATE [Users]
                SET [UserTypeId] = CASE
                    WHEN [UserType] IN (N'Engineer', N'2') THEN 2
                    ELSE 1
                END;");

            migrationBuilder.AlterColumn<int>(
                name: "UserTypeId",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTypeId",
                table: "Users",
                column: "UserTypeId");

            // Restrict: a type still assigned to users can never be deleted out from under them.
            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserTypes_UserTypeId",
                table: "Users",
                column: "UserTypeId",
                principalTable: "UserTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Users.UserType was never touched, so dropping the new column restores the exact
            // pre-migration state without any user losing their type.
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserTypes_UserTypeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserTypeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserTypeId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserTypes");
        }
    }
}
