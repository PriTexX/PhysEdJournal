using PhysEdJournal.Api;
using Serilog;
using Serilog.Formatting.Json;

static async Task Main(string[] args)
{
    var hostBuilder = CreateHostBuilder(args);

    hostBuilder.UseSerilog((context, configuration) =>
    {
        if (context.HostingEnvironment.IsProduction())
        {
            configuration.WriteTo.Console(new JsonFormatter());
        }
        else
        {
            configuration.WriteTo.Console();
        }
    });

    await hostBuilder.Build().RunAsync();
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        
await Main(args);