using Core.Commands.OldCommands;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Commands;

public static class DI
{
    public static void AddCommands(this IServiceCollection services)
    {
        services.AddScoped<AddPointsCommand>();
        services.AddScoped<AddVisitCommand>();
        services.AddScoped<AddStandardCommand>();
        services.AddScoped<ArchiveStudentCommand>();
        services.AddScoped<AssignCuratorCommand>();
        services.AddScoped<SyncStudentsCommand>();
        services.AddScoped<DeleteVisitCommand>();
        services.AddScoped<DeleteStandardCommand>();
        services.AddScoped<DeletePointsCommand>();
        services.AddScoped<AddHealthGroupCommand>();
    }
}
