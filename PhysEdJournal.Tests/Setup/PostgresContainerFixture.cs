using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Tests.Setup;

/// <summary>
/// Контейнер для базы данных postgres с миграциями и строкой подключения
/// </summary>
public class PostgresContainerFixture : IDisposable
{
    public static string ConnectionString { get; private set; }
    private static IContainer _pgContainer;

    public PostgresContainerFixture()
    {
        const string postgresPwd = "123456";

        _pgContainer = new ContainerBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithImage("postgres:15.3")
            .WithHostname(Guid.NewGuid().ToString("N"))
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_PASSWORD", postgresPwd)
            .WithEnvironment("PGDATA", "/pgdata")
            .WithTmpfsMount("/pgdata")
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilCommandIsCompleted("psql -U postgres -c \"select 1\"")
            )
            .Build();

        _pgContainer.StartAsync().GetAwaiter().GetResult();

        ConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = _pgContainer.Hostname,
            Port = _pgContainer.GetMappedPublicPort(5432),
            Password = postgresPwd,
            Database = "testDatabase",
            Username = "postgres"
        }.ConnectionString;

        var builder = new DbContextOptionsBuilder<ApplicationContext>().UseNpgsql(ConnectionString);
        var memoryCash = new MemoryCache(new MemoryCacheOptions());
        var dbContext = new ApplicationContext(builder.Options);

        dbContext.Database.Migrate();
        dbContext.Dispose();
    }

    public void Dispose()
    {
        _pgContainer.DisposeAsync().GetAwaiter().GetResult();
    }
}
