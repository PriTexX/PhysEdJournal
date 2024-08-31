﻿// <auto-generated />
using System;
using System.Collections.Generic;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20240831193456_specialization")]
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    partial class specialization
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DB.Tables.ArchivedStudentEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("character varying(120)");

                    b.Property<string>("GroupNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<List<ArchivedPointsHistory>>("PointsHistory")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("SemesterName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<List<ArchivedStandardsHistory>>("StandardsHistory")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("StudentGuid")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)");

                    b.Property<double>("TotalPoints")
                        .HasColumnType("double precision");

                    b.Property<int>("Visits")
                        .HasColumnType("integer");

                    b.Property<List<ArchivedHistory>>("VisitsHistory")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.HasIndex("GroupNumber");

                    b.HasIndex("SemesterName");

                    b.HasIndex("StudentGuid", "SemesterName")
                        .IsUnique();

                    b.ToTable("ArchivedStudents");
                });

            modelBuilder.Entity("DB.Tables.CompetitionEntity", b =>
                {
                    b.Property<string>("CompetitionName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("CompetitionName");

                    b.ToTable("Competitions");
                });

            modelBuilder.Entity("DB.Tables.GroupEntity", b =>
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

            modelBuilder.Entity("DB.Tables.PointsHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

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

                    b.HasIndex("StudentGuid");

                    b.HasIndex("TeacherGuid");

                    b.ToTable("PointsHistory");
                });

            modelBuilder.Entity("DB.Tables.SemesterEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("boolean");

                    b.HasKey("Name");

                    b.ToTable("Semesters");
                });

            modelBuilder.Entity("DB.Tables.StandardsHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

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

                    b.HasIndex("StudentGuid");

                    b.HasIndex("TeacherGuid");

                    b.ToTable("StandardsHistory");
                });

            modelBuilder.Entity("DB.Tables.StudentEntity", b =>
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

                    b.Property<bool>("HadDebtInSemester")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasDebt")
                        .HasColumnType("boolean");

                    b.Property<int>("HealthGroup")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<int>("PointsForStandards")
                        .HasColumnType("integer");

                    b.Property<int>("Specialization")
                        .HasColumnType("integer");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<int>("Visits")
                        .HasColumnType("integer");

                    b.HasKey("StudentGuid");

                    b.HasIndex("CurrentSemesterName");

                    b.HasIndex("GroupNumber");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("DB.Tables.TeacherEntity", b =>
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

            modelBuilder.Entity("DB.Tables.VisitsHistoryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

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

                    b.ToTable("VisitsHistory");
                });

            modelBuilder.Entity("DB.Tables.ArchivedStudentEntity", b =>
                {
                    b.HasOne("DB.Tables.GroupEntity", "Group")
                        .WithMany()
                        .HasForeignKey("GroupNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DB.Tables.SemesterEntity", "Semester")
                        .WithMany("ArchivedStudents")
                        .HasForeignKey("SemesterName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("DB.Tables.GroupEntity", b =>
                {
                    b.HasOne("DB.Tables.TeacherEntity", "Curator")
                        .WithMany("Groups")
                        .HasForeignKey("CuratorGuid");

                    b.Navigation("Curator");
                });

            modelBuilder.Entity("DB.Tables.PointsHistoryEntity", b =>
                {
                    b.HasOne("DB.Tables.StudentEntity", "Student")
                        .WithMany("PointsHistory")
                        .HasForeignKey("StudentGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DB.Tables.TeacherEntity", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("DB.Tables.StandardsHistoryEntity", b =>
                {
                    b.HasOne("DB.Tables.StudentEntity", "Student")
                        .WithMany("StandardsHistory")
                        .HasForeignKey("StudentGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DB.Tables.TeacherEntity", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("DB.Tables.StudentEntity", b =>
                {
                    b.HasOne("DB.Tables.SemesterEntity", "Semester")
                        .WithMany()
                        .HasForeignKey("CurrentSemesterName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DB.Tables.GroupEntity", "Group")
                        .WithMany("Students")
                        .HasForeignKey("GroupNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("DB.Tables.VisitsHistoryEntity", b =>
                {
                    b.HasOne("DB.Tables.StudentEntity", "Student")
                        .WithMany("VisitsHistory")
                        .HasForeignKey("StudentGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DB.Tables.TeacherEntity", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("DB.Tables.GroupEntity", b =>
                {
                    b.Navigation("Students");
                });

            modelBuilder.Entity("DB.Tables.SemesterEntity", b =>
                {
                    b.Navigation("ArchivedStudents");
                });

            modelBuilder.Entity("DB.Tables.StudentEntity", b =>
                {
                    b.Navigation("PointsHistory");

                    b.Navigation("StandardsHistory");

                    b.Navigation("VisitsHistory");
                });

            modelBuilder.Entity("DB.Tables.TeacherEntity", b =>
                {
                    b.Navigation("Groups");
                });
#pragma warning restore 612, 618
        }
    }
}
