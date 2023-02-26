using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Infrastructure.Database;

public sealed class ApplicationContext : DbContext
{
    public DbSet<GroupEntity> Groups { get; set; }
    public DbSet<PointsStudentHistoryEntity> StudentsPointsHistory { get; set; }
    public DbSet<VisitsStudentHistoryEntity> StudentsVisitsHistory { get; set; }
    public DbSet<StudentEntity> Students { get; set; }
    public DbSet<TeacherEntity> Teachers { get; set; }
    public DbSet<ArchivedStudentEntity> ArchivedStudents { get; set; }
    public DbSet<SemesterEntity> Semesters { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivedStudentEntity>()
            .HasKey(s => new { s.StudentGuid, s.SemesterName });

        base.OnModelCreating(modelBuilder);
    }
}