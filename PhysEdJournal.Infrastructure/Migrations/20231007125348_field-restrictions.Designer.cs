﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PhysEdJournal.Infrastructure.Database;

#nullable disable

namespace PhysEdJournal.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20231007125348_field-restrictions")]
    partial class fieldrestrictions
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.ArchivedStudentEntity", b =>
                {
                    b.Property<string>("StudentGuid")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<string>("SemesterName")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("character varying(120)");

                    b.Property<string>("GroupNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<double>("TotalPoints")
                        .HasColumnType("double precision");

                    b.Property<int>("Visits")
                        .HasColumnType("integer");

                    b.HasKey("StudentGuid", "SemesterName");

                    b.HasIndex("GroupNumber");

                    b.HasIndex("SemesterName");

                    b.ToTable("ArchivedStudents");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.CompetitionEntity", b =>
                {
                    b.Property<string>("CompetitionName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("CompetitionName");

                    b.ToTable("Competitions");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.GroupEntity", b =>
                {
                    b.Property<string>("GroupName")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("CuratorGuid")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<double>("VisitValue")
                        .HasColumnType("double precision");

                    b.HasKey("GroupName");

                    b.HasIndex("CuratorGuid");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.PointsStudentHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<string>("SemesterName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("StudentGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<string>("TeacherGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<int>("WorkType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SemesterName");

                    b.HasIndex("StudentGuid");

                    b.HasIndex("TeacherGuid");

                    b.ToTable("PointsStudentsHistory");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.SemesterEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("boolean");

                    b.HasKey("Name");

                    b.ToTable("Semesters");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.StandardsStudentHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<string>("SemesterName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<int>("StandardType")
                        .HasColumnType("integer");

                    b.Property<string>("StudentGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<string>("TeacherGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.HasKey("Id");

                    b.HasIndex("SemesterName");

                    b.HasIndex("StudentGuid");

                    b.HasIndex("TeacherGuid");

                    b.ToTable("StandardsStudentsHistory");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.StudentEntity", b =>
                {
                    b.Property<string>("StudentGuid")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<int>("AdditionalPoints")
                        .HasColumnType("integer");

                    b.Property<double>("ArchivedVisitValue")
                        .HasColumnType("double precision");

                    b.Property<int>("Course")
                        .HasColumnType("integer");

                    b.Property<string>("CurrentSemesterName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("Department")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("character varying(120)");

                    b.Property<string>("GroupNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<bool>("HasDebtFromPreviousSemester")
                        .HasColumnType("boolean");

                    b.Property<int>("HealthGroup")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<int>("PointsForStandards")
                        .HasColumnType("integer");

                    b.Property<int>("Visits")
                        .HasColumnType("integer");

                    b.HasKey("StudentGuid");

                    b.HasIndex("CurrentSemesterName");

                    b.HasIndex("GroupNumber");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.TeacherEntity", b =>
                {
                    b.Property<string>("TeacherGuid")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("character varying(120)");

                    b.Property<int>("Permissions")
                        .HasColumnType("integer");

                    b.HasKey("TeacherGuid");

                    b.ToTable("Teachers");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.VisitStudentHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean");

                    b.Property<string>("StudentGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<string>("TeacherGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.HasKey("Id");

                    b.HasIndex("StudentGuid");

                    b.HasIndex("TeacherGuid");

                    b.ToTable("VisitsStudentsHistory");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.ArchivedStudentEntity", b =>
                {
                    b.HasOne("PhysEdJournal.Core.Entities.DB.GroupEntity", "Group")
                        .WithMany()
                        .HasForeignKey("GroupNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.SemesterEntity", "Semester")
                        .WithMany("ArchivedStudents")
                        .HasForeignKey("SemesterName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.GroupEntity", b =>
                {
                    b.HasOne("PhysEdJournal.Core.Entities.DB.TeacherEntity", "Curator")
                        .WithMany("Groups")
                        .HasForeignKey("CuratorGuid");

                    b.Navigation("Curator");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.PointsStudentHistoryEntity", b =>
                {
                    b.HasOne("PhysEdJournal.Core.Entities.DB.SemesterEntity", "Semester")
                        .WithMany("StudentPointsHistory")
                        .HasForeignKey("SemesterName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.StudentEntity", "Student")
                        .WithMany("PointsStudentHistory")
                        .HasForeignKey("StudentGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.TeacherEntity", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Semester");

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.StandardsStudentHistoryEntity", b =>
                {
                    b.HasOne("PhysEdJournal.Core.Entities.DB.SemesterEntity", "Semester")
                        .WithMany()
                        .HasForeignKey("SemesterName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.StudentEntity", "Student")
                        .WithMany("StandardsStudentHistory")
                        .HasForeignKey("StudentGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.TeacherEntity", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Semester");

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.StudentEntity", b =>
                {
                    b.HasOne("PhysEdJournal.Core.Entities.DB.SemesterEntity", "Semester")
                        .WithMany()
                        .HasForeignKey("CurrentSemesterName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.GroupEntity", "Group")
                        .WithMany("Students")
                        .HasForeignKey("GroupNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.VisitStudentHistoryEntity", b =>
                {
                    b.HasOne("PhysEdJournal.Core.Entities.DB.StudentEntity", "Student")
                        .WithMany("VisitsStudentHistory")
                        .HasForeignKey("StudentGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PhysEdJournal.Core.Entities.DB.TeacherEntity", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.GroupEntity", b =>
                {
                    b.Navigation("Students");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.SemesterEntity", b =>
                {
                    b.Navigation("ArchivedStudents");

                    b.Navigation("StudentPointsHistory");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.StudentEntity", b =>
                {
                    b.Navigation("PointsStudentHistory");

                    b.Navigation("StandardsStudentHistory");

                    b.Navigation("VisitsStudentHistory");
                });

            modelBuilder.Entity("PhysEdJournal.Core.Entities.DB.TeacherEntity", b =>
                {
                    b.Navigation("Groups");
                });
#pragma warning restore 612, 618
        }
    }
}
