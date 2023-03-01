using System.Text.Json.Serialization;
using PhysEdJournal.Api.GraphQL;
using PhysEdJournal.Api.GraphQL.MutationExtensions;
using PhysEdJournal.Api.GraphQL.QueryExtensions;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Infrastructure;
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
        services.AddAuthorization();

        services
            .AddOptions<ApplicationOptions>()
            .BindConfiguration("Application");

        services.AddInfrastructure(Configuration);

        services
            .AddGraphQLServer()
            .AddMutationConventions(applyToAllMutations: true)
            .RegisterDbContext<ApplicationContext>()
            .AddQueryType<Query>()
            .AddType<SuccessType>()
            .AddType<DateOnlyType>()
            .BindRuntimeType<Success, SuccessType>()
            .BindRuntimeType<DateOnly, DateOnlyType>()
            .AddTypeExtension<TeacherQueryExtensions>()
            .AddMutationType<Mutation>()
            .AddTypeExtension<TeacherMutationExtensions>()
            .AddTypeExtension<GroupMutationExtensions>()
            .AddTypeExtension<SemesterMutationExtensions>()
            .AddTypeExtension<StudentMutationExtensions>()
            .AddProjections()
            .AddFiltering()
            .AddSorting();

        services.AddEndpointsApiExplorer();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
           
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGraphQL();
        });
    }
}