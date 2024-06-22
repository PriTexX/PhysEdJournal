using Coravel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Jobs;

public static class Worker
{
    public static void AddWorker(this IServiceCollection services)
    {
        services.AddScheduler();

        services.AddTransient<ArchiveStudentJob>();
        services.AddTransient<SyncStudentsJob>();
    }

    public static void UseWorker(this WebApplication app)
    {
        app.Services.UseScheduler(scheduler =>
        {
            scheduler.Schedule<ArchiveStudentJob>().DailyAtHour(3);
            scheduler.Schedule<SyncStudentsJob>().DailyAtHour(4);
        });
    }
}
