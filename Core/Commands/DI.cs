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
        services.AddScoped<SyncStudentsCommand>();
        services.AddScoped<CreateCompetitionCommand>();
        services.AddScoped<DeleteCompetitionCommand>();
        services.AddScoped<DeleteVisitCommand>();
        services.AddScoped<DeleteStandardCommand>();
        services.AddScoped<DeletePointsCommand>();
        services.AddScoped<AddHealthGroupCommand>();
    }
}
