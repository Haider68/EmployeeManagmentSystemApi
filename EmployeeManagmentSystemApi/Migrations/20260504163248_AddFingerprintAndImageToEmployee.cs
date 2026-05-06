using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagmentSystemApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFingerprintAndImageToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FingerPrint",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FingerPrint",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Employees");
        }
    }
}
