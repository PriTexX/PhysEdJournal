using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using PhysEdJournal.Infrastructure.Validators.Standards;

namespace PhysEdJournal.Infrastructure.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<StandardsValidator>();
        services.AddScoped<PermissionValidator>();

        services.AddMemoryCache();
            
        services.AddScoped<GroupService>();
        services.AddScoped<SemesterService>();
        services.AddScoped<StudentService>();
        services.AddScoped<TeacherService>();

        return services;
    }
}