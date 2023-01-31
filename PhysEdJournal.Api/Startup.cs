using System.Text.Json.Serialization;

namespace PhysEdJournal.Api;

public class Startup
{
    private IConfiguration Configuration { get; }
    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddDateOnlyTimeOnlyStringConverters();
        
        services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.UseDateOnlyTimeOnlyStringConverters());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}