using System.Text.Json.Serialization;
using PhysEdJournal.Api.GraphQL;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.DI;

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

        services.AddInfrastructure(Configuration);

        services
            .AddGraphQLServer()
            .RegisterDbContext<ApplicationContext>()
            .AddQueryType<Query>()
            .AddProjections()
            .AddFiltering()
            .AddSorting();
        
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
        
           
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGraphQL();
            endpoints.MapControllers();
        });
    }
}