using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Standards;

namespace PhysEdJournal.Infrastructure.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<StandardsValidator>();

        services.AddMemoryCache();

        services.AddCommands();

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
    }

}