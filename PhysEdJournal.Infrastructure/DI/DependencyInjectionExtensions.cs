using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services.Quartz.Jobs;
using Quartz;
using Quartz.AspNetCore;
using Quartz.Simpl;

namespace PhysEdJournal.Infrastructure.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ApplicationContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

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
        services.AddScoped<UpdateStudentsInfoCommand>();
        services.AddScoped<DeleteStudentVisitCommand>();
        services.AddScoped<DeleteStandardPointsCommand>();
        services.AddScoped<DeletePointsCommand>();
        services.AddScoped<MigrateToNextSemesterCommand>();
        services.AddScoped<UpdateGroupsVisitValueCommand>();
    }

    private static void AddMyQuartz(this IServiceCollection services)
    {
        services.AddScoped<ArchiveStudentJob>();

        services.AddQuartz(q =>
        {
            q.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();
            var jobKey = new JobKey("ArchiveStudentJob");
            q.AddJob<ArchiveStudentJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(
                opts =>
                    opts.ForJob(jobKey)
                        .WithIdentity("ArchiveStudentJob-trigger")
                        .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(6, 0))
            );
        });
        services.AddQuartzServer(q => q.WaitForJobsToComplete = true);
    }
}
