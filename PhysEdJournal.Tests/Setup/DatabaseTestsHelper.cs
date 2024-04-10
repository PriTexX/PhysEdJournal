using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Tests.Setup;

[Collection("Db collection")]
public abstract class DatabaseTestsHelper
{
    protected static ApplicationContext CreateContext()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkNpgsql()
            .BuildServiceProvider();

        var dataSource = new NpgsqlDataSourceBuilder(PostgresContainerFixture.ConnectionString)
            .EnableDynamicJson()
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(dataSource)
            .UseInternalServiceProvider(serviceProvider);

        var dbContext = new ApplicationContext(builder.Options);
        return dbContext;
    }

    protected static IMemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    protected static async Task ClearDatabase(ApplicationContext context)
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
