using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Jobs;
using Quartz;
using Quartz.AspNetCore;
using Quartz.Simpl;

namespace PhysEdJournal.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var dataSource = new NpgsqlDataSourceBuilder(
            configuration.GetConnectionString("DefaultConnection")
        )
            .EnableDynamicJson()
            .Build();

        services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(dataSource));

        services.AddMemoryCache();

        services.AddCommands();

        services.AddMyQuartz();

        return services;
    }

    private static void AddCommands(this IServiceCollection services)
    {
        services.AddScoped<AddPointsCommand>();
        services.AddScoped<IncreaseStudentVisitsCommand>();
        services.AddScoped<AddStandardPointsCommand>();
        services.AddScoped<ActivateStudentCommand>();
        services.AddScoped<DeActivateStudentCommand>();
        services.AddScoped<ArchiveStudentCommand>();
        services.AddScoped<UnArchiveStudentCommand>();
        services.AddScoped<AssignCuratorCommand>();
        services.AddScoped<AssignVisitValueCommand>();
        services.AddScoped<CreateCompetitionCommand>();
        services.AddScoped<CreateTeacherCommand>();
        services.AddScoped<DeleteCompetitionCommand>();
        services.AddScoped<GivePermissionsCommand>();
        services.AddScoped<StartNewSemesterCommand>();
        services.AddScoped<SyncStudentsCommand>();
        services.AddScoped<DeleteStudentVisitCommand>();
        services.AddScoped<DeleteStandardPointsCommand>();
        services.AddScoped<DeletePointsCommand>();
        services.AddScoped<MigrateToNextSemesterCommand>();
    }

    private static void AddMyQuartz(this IServiceCollection services)
    {
        services.AddScoped<ArchiveStudentJob>();
        services.AddScoped<SyncStudentsJob>();

        services.AddQuartz(q =>
        {
            q.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();

            var archiveStudentsJobKey = new JobKey(nameof(ArchiveStudentJob));
            q.AddJob<ArchiveStudentJob>(opts => opts.WithIdentity(archiveStudentsJobKey));

            q.AddTrigger(opts =>
                opts.ForJob(archiveStudentsJobKey)
                    .WithIdentity($"${nameof(ArchiveStudentJob)}-trigger")
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(6, 0))
            );

            var syncStudentsJobKey = new JobKey(nameof(SyncStudentsJob));
            q.AddJob<SyncStudentsJob>(opts => opts.WithIdentity(syncStudentsJobKey));

            q.AddTrigger(opts =>
                opts.ForJob(syncStudentsJobKey)
                    .WithIdentity($"{nameof(SyncStudentsJob)}-trigger")
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(6, 0))
            );
        });
        services.AddQuartzServer(q => q.WaitForJobsToComplete = true);
    }
}
