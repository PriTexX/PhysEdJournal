﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhysEdJournal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class numeric_archived_students_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchivedStudents",
                table: "ArchivedStudents");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ArchivedStudents",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchivedStudents",
                table: "ArchivedStudents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedStudents_StudentGuid_SemesterName",
                table: "ArchivedStudents",
                columns: new[] { "StudentGuid", "SemesterName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchivedStudents",
                table: "ArchivedStudents");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedStudents_StudentGuid_SemesterName",
                table: "ArchivedStudents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArchivedStudents");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchivedStudents",
                table: "ArchivedStudents",
                columns: new[] { "StudentGuid", "SemesterName" });
        }
    }
}
