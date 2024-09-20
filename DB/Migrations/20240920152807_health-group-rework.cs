using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable SA1300
    public partial class healthgrouprework : Migration
#pragma warning restore SA1300
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HealthGroupProviderTeacherGuid",
                table: "Students",
                type: "character varying(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_HealthGroupProviderTeacherGuid",
                table: "Students",
                column: "HealthGroupProviderTeacherGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Teachers_HealthGroupProviderTeacherGuid",
                table: "Students",
                column: "HealthGroupProviderTeacherGuid",
                principalTable: "Teachers",
                principalColumn: "TeacherGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Teachers_HealthGroupProviderTeacherGuid",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_HealthGroupProviderTeacherGuid",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "HealthGroupProviderTeacherGuid",
                table: "Students");
        }
    }
}
