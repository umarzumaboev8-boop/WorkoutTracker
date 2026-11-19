using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkoutTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SportTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Calories = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    SportTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PhotoFileName = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workouts_SportTypes_SportTypeId",
                        column: x => x.SportTypeId,
                        principalTable: "SportTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SportTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Boxing" },
                    { 2, "Running" },
                    { 3, "Swimming" },
                    { 4, "Gym" },
                    { 5, "Cycling" }
                });

            migrationBuilder.InsertData(
                table: "Workouts",
                columns: new[] { "Id", "AuthorId", "Calories", "Date", "DurationMinutes", "Notes", "PhotoFileName", "SportTypeId", "Title" },
                values: new object[,]
                {
                    { 1, "demo-user", 450, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 60, null, null, 1, "Morning Boxing" },
                    { 2, "demo-user", 300, new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 40, null, null, 2, "Morning Run" },
                    { 3, "demo-user", 400, new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 50, null, null, 3, "Swimming Session" },
                    { 4, "demo-user", 500, new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 75, null, null, 4, "Gym Training" },
                    { 5, "demo-user", 600, new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 90, null, null, 5, "Cycling Session" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_SportTypeId",
                table: "Workouts",
                column: "SportTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workouts");

            migrationBuilder.DropTable(
                name: "SportTypes");
        }
    }
}
