using System.Security.Cryptography;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PhysEdJournal.Api;
using PhysEdJournal.Api.Endpoints.MeEndpoint;
using PhysEdJournal.Api.Endpoints.StaffEndpoint;
using PhysEdJournal.Api.FilterExtensions;
using PhysEdJournal.Api.GraphQL;
using PhysEdJournal.Api.GraphQL.MutationExtensions;
using PhysEdJournal.Api.GraphQL.QueryExtensions;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Api.Middlewares;
using PhysEdJournal.Api.Rest.Competition;
using PhysEdJournal.Api.Rest.Group;
using PhysEdJournal.Api.Rest.Points;
using PhysEdJournal.Api.Rest.Semester;
using PhysEdJournal.Api.Rest.Student;
using PhysEdJournal.Api.Rest.Teacher;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

/*
    Application options
 */

builder
    .Services.AddOptions<ApplicationOptions>()
    .BindConfiguration(ApplicationOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var applicationOptions = new ApplicationOptions();
builder.Configuration.GetSection(ApplicationOptions.SectionName).Bind(applicationOptions);

/*
    Logging
 */

builder.Host.UseSerilog(
    (context, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database", LogEventLevel.Warning)
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

/*
    Authentication & Authorization
 */

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = GetSecurityKey(applicationOptions.RsaPublicKey),
            ValidIssuer = "humanresourcesdepartmentapi.mospolytech.ru",
            ValidAudience = "HumanResourcesDepartment",
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
        }
    );

builder.Services.AddAuthorization();

/*
    Services
 */

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<IStaffInfoClient, StaffInfoHttpClient>();
builder.Services.AddScoped<PermissionValidator>();
builder.Services.AddScoped<MeInfoService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(swagger =>
    {
        swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "PhysEdJournal", Version = "v1" });

        swagger.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            }
        );

        swagger.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            }
        );
    });
}

/*
    Utils
 */

builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();

/*
    GraphQL
 */

builder
    .Services.AddGraphQLServer()
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
        new FilterConventionExtension(x =>
            x.AddProviderExtension(
                new QueryableFilterProviderExtension(y =>
                    y.AddFieldHandler<QueryableStringInvariantContainsHandler>()
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
            IncludeTotalCount = true,
        }
    );

var app = builder.Build();

app.UseRequestId();

/*
    Rest
 */

// var root = app.MapGroup("/api");
//
// PointsController.MapEndpoints(root);
// GroupController.MapEndpoints(root);
// CompetitionController.MapEndpoints(root);
// SemesterController.MapEndpoints(root);
// StudentController.MapEndpoints(root);
// TeacherController.MapEndpoints(root);

/*
    Middlewares
 */

app.UseHttpsRedirection();

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder.AllowAnyOrigin();
    corsPolicyBuilder.AllowAnyHeader();
});

app.UseRouting();

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    });
}

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseUserGuidLogger();
app.MapControllers();
app.MapGraphQL();

await app.RunAsync();

static RsaSecurityKey GetSecurityKey(string publicKey)
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(publicKey);
    return new RsaSecurityKey(rsa);
}
