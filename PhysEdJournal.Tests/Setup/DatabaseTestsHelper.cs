using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Tests.Setup;

[Collection("Db collection")]
public abstract class DatabaseTestsHelper
{
    internal static ApplicationContext CreateContext(IMemoryCache cache = null)
    {
        var builder = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(PostgresContainerFixture.ConnectionString);
        var memoryCash = cache ?? new MemoryCache(new MemoryCacheOptions());
        var dbContext = new ApplicationContext(builder.Options, memoryCash);
        return dbContext;
    }

    internal static IMemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    internal static async Task ClearDatabase(ApplicationContext context)
    {
        await context.Groups.ExecuteDeleteAsync();
        await context.PointsStudentsHistory.ExecuteDeleteAsync();
        await context.VisitsStudentsHistory.ExecuteDeleteAsync();
        await context.StandardsStudentsHistory.ExecuteDeleteAsync();
        await context.Students.ExecuteDeleteAsync();
        await context.Teachers.ExecuteDeleteAsync();
        await context.ArchivedStudents.ExecuteDeleteAsync();
        await context.Semesters.ExecuteDeleteAsync();
        await context.Competitions.ExecuteDeleteAsync();
    }
}