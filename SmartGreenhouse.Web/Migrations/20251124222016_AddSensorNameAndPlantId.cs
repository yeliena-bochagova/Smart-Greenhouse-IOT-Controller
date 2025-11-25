using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartGreenhouse.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorNameAndPlantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MaxHumidity",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxLight",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxTemperature",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinHumidity",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinLight",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinTemperature",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Sensors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_UserId",
                table: "Sensors",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Users_UserId",
                table: "Sensors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Users_UserId",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_UserId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MaxHumidity",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MaxLight",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MaxTemperature",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MinHumidity",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MinLight",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MinTemperature",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Sensors");
        }
    }
}
