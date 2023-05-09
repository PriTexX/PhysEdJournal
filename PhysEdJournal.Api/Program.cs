using PhysEdJournal.Api;
using Serilog;

static async Task Main(string[] args)
{
    var hostBuilder = CreateHostBuilder(args);

    hostBuilder.UseSerilog((context, configuration) =>
    {
        configuration.WriteTo.Console();
    });

    await hostBuilder.Build().RunAsync();
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        
await Main(args);