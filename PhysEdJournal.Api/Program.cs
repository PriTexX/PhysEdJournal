using PhysEdJournal.Api;

static async Task Main(string[] args)
{
    await CreateHostBuilder(args).Build().RunAsync(); 
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        
await Main(args);