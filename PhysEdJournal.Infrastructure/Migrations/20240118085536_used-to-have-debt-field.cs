using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhysEdJournal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Usedtohavedebtfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UsedToHaveDebtInCurrentSemester",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UsedToHaveDebtInCurrentSemester", table: "Students");
        }
    }
}
