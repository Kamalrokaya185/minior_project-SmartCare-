using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.Infrastructure.Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "DoctorProfile");
        }
    }
}
