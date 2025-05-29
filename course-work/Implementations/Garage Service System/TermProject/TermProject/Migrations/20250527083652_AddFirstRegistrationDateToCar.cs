using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TermProject.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstRegistrationDateToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Year",
                table: "Cars",
                newName: "ManufactureYear");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Cars",
                newName: "FirstRegistrationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ManufactureYear",
                table: "Cars",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "FirstRegistrationDate",
                table: "Cars",
                newName: "RegistrationDate");
        }
    }
}
