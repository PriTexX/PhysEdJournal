using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Infrastructure.Database;

public sealed class ApplicationContext : DbContext
{
    public DbSet<GroupEntity> Groups { get; init; } = null!;
    public DbSet<PointsStudentHistoryEntity> PointsStudentsHistory { get; init; } = null!;
    public DbSet<VisitStudentHistoryEntity> VisitsStudentsHistory { get; init; } = null!;
    public DbSet<StandardsStudentHistoryEntity> StandardsStudentsHistory { get; init; } = null!;
    public DbSet<StudentEntity> Students { get; init; } = null!;
    public DbSet<TeacherEntity> Teachers { get; init; } = null!;
    public DbSet<ArchivedStudentEntity> ArchivedStudents { get; init; } = null!;
    public DbSet<SemesterEntity> Semesters { get; init; } = null!;

    public DbSet<CompetitionEntity> Competitions { get; init; } = null!;

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivedStudentEntity>(e =>
            e.HasIndex(a => new { a.StudentGuid, a.SemesterName }).IsUnique()
        );
        base.OnModelCreating(modelBuilder);
    }

    public async ValueTask<SemesterEntity> GetActiveSemester()
    {
        return await Semesters.Where(s => s.IsCurrent).SingleAsync();
    }
}
