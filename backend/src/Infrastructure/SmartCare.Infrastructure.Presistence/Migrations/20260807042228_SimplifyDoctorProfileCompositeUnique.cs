using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCare.Infrastructure.Presistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyDoctorProfileCompositeUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorProfile_LicenseNumber",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "Biography",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "ExperienceYear",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "DoctorProfile");

            migrationBuilder.AlterColumn<string>(
                name: "Specialization",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfile_LicenseNumber_Specialization",
                table: "DoctorProfile",
                columns: new[] { "LicenseNumber", "Specialization" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorProfile_LicenseNumber_Specialization",
                table: "DoctorProfile");

            migrationBuilder.AlterColumn<string>(
                name: "Specialization",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "DoctorProfile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYear",
                table: "DoctorProfile",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DoctorProfile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Qualification",
                table: "DoctorProfile",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfile_LicenseNumber",
                table: "DoctorProfile",
                column: "LicenseNumber",
                unique: true);
        }
    }
}
