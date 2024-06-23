﻿// <auto-generated />
using System.Collections.Generic;
using DB.Tables;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class archive_student : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointsStudentsHistory_Semesters_SemesterName",
                table: "PointsStudentsHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_StandardsStudentsHistory_Semesters_SemesterName",
                table: "StandardsStudentsHistory");

            migrationBuilder.DropIndex(
                name: "IX_StandardsStudentsHistory_SemesterName",
                table: "StandardsStudentsHistory");

            migrationBuilder.DropIndex(
                name: "IX_PointsStudentsHistory_SemesterName",
                table: "PointsStudentsHistory");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "VisitsStudentsHistory");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "StandardsStudentsHistory");

            migrationBuilder.DropColumn(
                name: "SemesterName",
                table: "StandardsStudentsHistory");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "PointsStudentsHistory");

            migrationBuilder.DropColumn(
                name: "SemesterName",
                table: "PointsStudentsHistory");

            migrationBuilder.AddColumn<List<PointsHistoryEntity>>(
                name: "PointsHistory",
                table: "ArchivedStudents",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<List<StandardsHistoryEntity>>(
                name: "StandardsHistory",
                table: "ArchivedStudents",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<List<VisitsHistoryEntity>>(
                name: "VisitsHistory",
                table: "ArchivedStudents",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointsHistory",
                table: "ArchivedStudents");

            migrationBuilder.DropColumn(
                name: "StandardsHistory",
                table: "ArchivedStudents");

            migrationBuilder.DropColumn(
                name: "VisitsHistory",
                table: "ArchivedStudents");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "VisitsStudentsHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "StandardsStudentsHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SemesterName",
                table: "StandardsStudentsHistory",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "PointsStudentsHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SemesterName",
                table: "PointsStudentsHistory",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StandardsStudentsHistory_SemesterName",
                table: "StandardsStudentsHistory",
                column: "SemesterName");

            migrationBuilder.CreateIndex(
                name: "IX_PointsStudentsHistory_SemesterName",
                table: "PointsStudentsHistory",
                column: "SemesterName");

            migrationBuilder.AddForeignKey(
                name: "FK_PointsStudentsHistory_Semesters_SemesterName",
                table: "PointsStudentsHistory",
                column: "SemesterName",
                principalTable: "Semesters",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StandardsStudentsHistory_Semesters_SemesterName",
                table: "StandardsStudentsHistory",
                column: "SemesterName",
                principalTable: "Semesters",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}