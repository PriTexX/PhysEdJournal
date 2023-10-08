﻿using System.Security.Cryptography;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PhysEdJournal.Api.Endpoints.MeEndpoint;
using PhysEdJournal.Api.Endpoints.StaffEndpoint;
using PhysEdJournal.Api.FilterExtensions;
using PhysEdJournal.Api.GraphQL;
using PhysEdJournal.Api.GraphQL.MutationExtensions;
using PhysEdJournal.Api.GraphQL.QueryExtensions;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Api.Monitoring.Logging;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.DI;
using Prometheus;
using Serilog;

namespace PhysEdJournal.Api;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private static RsaSecurityKey GetSecurityKey(string publicKey)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return new RsaSecurityKey(rsa);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOptions<ApplicationOptions>()
            .BindConfiguration(ApplicationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = GetSecurityKey(
                            Configuration["Application:RsaPublicKey"]
                        ),
                        ValidIssuer = "humanresourcesdepartmentapi.mospolytech.ru",
                        ValidAudience = "HumanResourcesDepartment",
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    }
            );

        services.AddControllers();
        services.AddAuthorization();
        services.AddSingleton<IStaffInfoClient, StaffInfoHttpClient>();
        services.AddCors();

        services.AddInfrastructure(Configuration);

        services.AddScoped<PermissionValidator>();
        services.AddScoped<MeInfoService>();

        services
            .AddGraphQLServer()
            .InitializeOnStartup()
            .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>()
            .AddAuthorization()
            .AddMutationConventions(applyToAllMutations: true)
            .RegisterDbContext<ApplicationContext>()
            .AddQueryType<Query>()
            .AddTypeExtension<TeacherQueryExtensions>()
            .AddTypeExtension<StudentQueryExtensions>()
            .AddType<SuccessType>()
            .AddType<DateOnlyType>()
            .BindRuntimeType<Success, SuccessType>()
            .BindRuntimeType<DateOnly, DateOnlyType>()
            .AddMutationType<Mutation>()
            .AddTypeExtension<TeacherMutationExtensions>()
            .AddTypeExtension<GroupMutationExtensions>()
            .AddTypeExtension<SemesterMutationExtensions>()
            .AddTypeExtension<StudentMutationExtensions>()
            .AddProjections()
            .AddFiltering()
            .AddConvention<IFilterConvention>(
                new FilterConventionExtension(
                    x =>
                        x.AddProviderExtension(
                            new QueryableFilterProviderExtension(
                                y => y.AddFieldHandler<QueryableStringInvariantContainsHandler>()
                            )
                        )
                )
            )
            .AddSorting()
            .SetPagingOptions(
                new PagingOptions
                {
                    MaxPageSize = 200,
                    DefaultPageSize = 30,
                    IncludeTotalCount = true
                }
            );

        services.AddEndpointsApiExplorer();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin();
            builder.AllowAnyHeader();
        });
        app.UseMetricServer();

        app.UseSerilogRequestLogging();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseUserGuidLogger();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGraphQL();
        });
    }
}
