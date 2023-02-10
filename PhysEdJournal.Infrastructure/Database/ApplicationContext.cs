using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Infrastructure.Database;

public class ApplicationContext : DbContext
{
    public DbSet<GroupEntity> Groups { get; set; }
    public DbSet<StudentPointsHistoryEntity> StudentsPointsHistory { get; set; }
    public DbSet<StudentVisitsHistoryEntity> StudentsVisitsHistory { get; set; }
    public DbSet<StudentEntity> Students { get; set; }
    public DbSet<TeacherEntity> Teachers { get; set; }
    public DbSet<ArchivedStudentEntity> ArchivedStudents { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }
}