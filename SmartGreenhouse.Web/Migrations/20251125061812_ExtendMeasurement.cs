using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartGreenhouse.Web.Migrations
{
    /// <inheritdoc />
    public partial class ExtendMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Sensors_SensorId",
                table: "Readings");

            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Users_UserId",
                table: "Readings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Readings",
                table: "Readings");

            migrationBuilder.RenameTable(
                name: "Readings",
                newName: "GreenhouseReading");

            migrationBuilder.RenameIndex(
                name: "IX_Readings_UserId",
                table: "GreenhouseReading",
                newName: "IX_GreenhouseReading_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Readings_SensorId",
                table: "GreenhouseReading",
                newName: "IX_GreenhouseReading_SensorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GreenhouseReading",
                table: "GreenhouseReading",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SensorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Temperature = table.Column<double>(type: "REAL", nullable: false),
                    Humidity = table.Column<double>(type: "REAL", nullable: false),
                    Light = table.Column<double>(type: "REAL", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_GreenhouseReading_Sensors_SensorId",
                table: "GreenhouseReading",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GreenhouseReading_Users_UserId",
                table: "GreenhouseReading",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GreenhouseReading_Sensors_SensorId",
                table: "GreenhouseReading");

            migrationBuilder.DropForeignKey(
                name: "FK_GreenhouseReading_Users_UserId",
                table: "GreenhouseReading");

            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GreenhouseReading",
                table: "GreenhouseReading");

            migrationBuilder.RenameTable(
                name: "GreenhouseReading",
                newName: "Readings");

            migrationBuilder.RenameIndex(
                name: "IX_GreenhouseReading_UserId",
                table: "Readings",
                newName: "IX_Readings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GreenhouseReading_SensorId",
                table: "Readings",
                newName: "IX_Readings_SensorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Readings",
                table: "Readings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Sensors_SensorId",
                table: "Readings",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Users_UserId",
                table: "Readings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
