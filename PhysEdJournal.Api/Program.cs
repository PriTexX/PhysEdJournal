using PhysEdJournal.Api;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

static async Task Main(string[] args)
{
    var hostBuilder = CreateHostBuilder(args);

    hostBuilder.UseSerilog(
        (context, configuration) =>
        {
            configuration.MinimumLevel
                .Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override(
                    "Microsoft.EntityFrameworkCore.Database",
                    LogEventLevel.Warning
                )
                .MinimumLevel.Override(
                    "Microsoft.AspNetCore.Hosting.Diagnostics",
                    LogEventLevel.Warning
                )
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Error)
                .Enrich.FromLogContext();

            if (context.HostingEnvironment.IsProduction())
            {
                configuration.WriteTo.Console(new JsonFormatter());
            }
            else
            {
                configuration.WriteTo.Console();
            }
        }
    );

    await hostBuilder.Build().RunAsync();
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

await Main(args);
