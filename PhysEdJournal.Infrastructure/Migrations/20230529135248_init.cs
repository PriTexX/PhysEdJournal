using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhysEdJournal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competitions",
                columns: table =>
                    new { CompetitionName = table.Column<string>(type: "text", nullable: false) },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.CompetitionName);
                }
            );

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table =>
                    new
                    {
                        Name = table.Column<string>(type: "text", nullable: false),
                        IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Name);
                }
            );

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table =>
                    new
                    {
                        TeacherGuid = table.Column<string>(type: "text", nullable: false),
                        FullName = table.Column<string>(type: "text", nullable: false),
                        Permissions = table.Column<int>(type: "integer", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherGuid);
                }
            );

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table =>
                    new
                    {
                        GroupName = table.Column<string>(type: "text", nullable: false),
                        VisitValue = table.Column<double>(
                            type: "double precision",
                            nullable: false
                        ),
                        CuratorGuid = table.Column<string>(type: "text", nullable: true)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupName);
                    table.ForeignKey(
                        name: "FK_Groups_Teachers_CuratorGuid",
                        column: x => x.CuratorGuid,
                        principalTable: "Teachers",
                        principalColumn: "TeacherGuid"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ArchivedStudents",
                columns: table =>
                    new
                    {
                        StudentGuid = table.Column<string>(type: "text", nullable: false),
                        SemesterName = table.Column<string>(type: "text", nullable: false),
                        FullName = table.Column<string>(type: "text", nullable: false),
                        GroupNumber = table.Column<string>(type: "text", nullable: false),
                        TotalPoints = table.Column<double>(
                            type: "double precision",
                            nullable: false
                        ),
                        Visits = table.Column<int>(type: "integer", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ArchivedStudents",
                        x => new { x.StudentGuid, x.SemesterName }
                    );
                    table.ForeignKey(
                        name: "FK_ArchivedStudents_Groups_GroupNumber",
                        column: x => x.GroupNumber,
                        principalTable: "Groups",
                        principalColumn: "GroupName",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ArchivedStudents_Semesters_SemesterName",
                        column: x => x.SemesterName,
                        principalTable: "Semesters",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table =>
                    new
                    {
                        StudentGuid = table.Column<string>(type: "text", nullable: false),
                        FullName = table.Column<string>(type: "text", nullable: false),
                        GroupNumber = table.Column<string>(type: "text", nullable: false),
                        HasDebtFromPreviousSemester = table.Column<bool>(
                            type: "boolean",
                            nullable: false
                        ),
                        ArchivedVisitValue = table.Column<double>(
                            type: "double precision",
                            nullable: false
                        ),
                        AdditionalPoints = table.Column<int>(type: "integer", nullable: false),
                        PointsForStandards = table.Column<int>(type: "integer", nullable: false),
                        IsActive = table.Column<bool>(type: "boolean", nullable: false),
                        Visits = table.Column<int>(type: "integer", nullable: false),
                        Course = table.Column<int>(type: "integer", nullable: false),
                        CurrentSemesterName = table.Column<string>(type: "text", nullable: false),
                        HealthGroup = table.Column<int>(type: "integer", nullable: false),
                        Department = table.Column<string>(type: "text", nullable: true)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentGuid);
                    table.ForeignKey(
                        name: "FK_Students_Groups_GroupNumber",
                        column: x => x.GroupNumber,
                        principalTable: "Groups",
                        principalColumn: "GroupName",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Students_Semesters_CurrentSemesterName",
                        column: x => x.CurrentSemesterName,
                        principalTable: "Semesters",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PointsStudentsHistory",
                columns: table =>
                    new
                    {
                        Id = table
                            .Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            ),
                        Date = table.Column<DateOnly>(type: "date", nullable: false),
                        Points = table.Column<int>(type: "integer", nullable: false),
                        SemesterName = table.Column<string>(type: "text", nullable: false),
                        TeacherGuid = table.Column<string>(type: "text", nullable: false),
                        WorkType = table.Column<int>(type: "integer", nullable: false),
                        IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                        Comment = table.Column<string>(type: "text", nullable: true),
                        StudentGuid = table.Column<string>(type: "text", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsStudentsHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsStudentsHistory_Semesters_SemesterName",
                        column: x => x.SemesterName,
                        principalTable: "Semesters",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PointsStudentsHistory_Students_StudentGuid",
                        column: x => x.StudentGuid,
                        principalTable: "Students",
                        principalColumn: "StudentGuid",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PointsStudentsHistory_Teachers_TeacherGuid",
                        column: x => x.TeacherGuid,
                        principalTable: "Teachers",
                        principalColumn: "TeacherGuid",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "StandardsStudentsHistory",
                columns: table =>
                    new
                    {
                        Id = table
                            .Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            ),
                        Points = table.Column<int>(type: "integer", nullable: false),
                        Date = table.Column<DateOnly>(type: "date", nullable: false),
                        SemesterName = table.Column<string>(type: "text", nullable: false),
                        IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                        StandardType = table.Column<int>(type: "integer", nullable: false),
                        TeacherGuid = table.Column<string>(type: "text", nullable: false),
                        StudentGuid = table.Column<string>(type: "text", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardsStudentsHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardsStudentsHistory_Semesters_SemesterName",
                        column: x => x.SemesterName,
                        principalTable: "Semesters",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_StandardsStudentsHistory_Students_StudentGuid",
                        column: x => x.StudentGuid,
                        principalTable: "Students",
                        principalColumn: "StudentGuid",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_StandardsStudentsHistory_Teachers_TeacherGuid",
                        column: x => x.TeacherGuid,
                        principalTable: "Teachers",
                        principalColumn: "TeacherGuid",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "VisitsStudentsHistory",
                columns: table =>
                    new
                    {
                        Id = table
                            .Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            ),
                        Date = table.Column<DateOnly>(type: "date", nullable: false),
                        TeacherGuid = table.Column<string>(type: "text", nullable: false),
                        IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                        StudentGuid = table.Column<string>(type: "text", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitsStudentsHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitsStudentsHistory_Students_StudentGuid",
                        column: x => x.StudentGuid,
                        principalTable: "Students",
                        principalColumn: "StudentGuid",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_VisitsStudentsHistory_Teachers_TeacherGuid",
                        column: x => x.TeacherGuid,
                        principalTable: "Teachers",
                        principalColumn: "TeacherGuid",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedStudents_GroupNumber",
                table: "ArchivedStudents",
                column: "GroupNumber"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedStudents_SemesterName",
                table: "ArchivedStudents",
                column: "SemesterName"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CuratorGuid",
                table: "Groups",
                column: "CuratorGuid"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PointsStudentsHistory_SemesterName",
                table: "PointsStudentsHistory",
                column: "SemesterName"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PointsStudentsHistory_StudentGuid",
                table: "PointsStudentsHistory",
                column: "StudentGuid"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PointsStudentsHistory_TeacherGuid",
                table: "PointsStudentsHistory",
                column: "TeacherGuid"
            );

            migrationBuilder.CreateIndex(
                name: "IX_StandardsStudentsHistory_SemesterName",
                table: "StandardsStudentsHistory",
                column: "SemesterName"
            );

            migrationBuilder.CreateIndex(
                name: "IX_StandardsStudentsHistory_StudentGuid",
                table: "StandardsStudentsHistory",
                column: "StudentGuid"
            );

            migrationBuilder.CreateIndex(
                name: "IX_StandardsStudentsHistory_TeacherGuid",
                table: "StandardsStudentsHistory",
                column: "TeacherGuid"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Students_CurrentSemesterName",
                table: "Students",
                column: "CurrentSemesterName"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Students_GroupNumber",
                table: "Students",
                column: "GroupNumber"
            );

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStudentsHistory_StudentGuid",
                table: "VisitsStudentsHistory",
                column: "StudentGuid"
            );

            migrationBuilder.CreateIndex(
                name: "IX_VisitsStudentsHistory_TeacherGuid",
                table: "VisitsStudentsHistory",
                column: "TeacherGuid"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ArchivedStudents");

            migrationBuilder.DropTable(name: "Competitions");

            migrationBuilder.DropTable(name: "PointsStudentsHistory");

            migrationBuilder.DropTable(name: "StandardsStudentsHistory");

            migrationBuilder.DropTable(name: "VisitsStudentsHistory");

            migrationBuilder.DropTable(name: "Students");

            migrationBuilder.DropTable(name: "Groups");

            migrationBuilder.DropTable(name: "Semesters");

            migrationBuilder.DropTable(name: "Teachers");
        }
    }
}
