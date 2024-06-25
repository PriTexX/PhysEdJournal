using System.Security.Cryptography;
using Api;
using Api.Endpoints.MeEndpoint;
using Api.Middlewares;
using Core.Cfg;
using Core.Commands;
using Core.Jobs;
using Core.Logging;
using DB;
using GraphQL.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

/*
    Application options
 */

Config.InitCoreCfg(builder);

/*
    Logging
 */

builder.AddLogging("Journal");

/*
    Authentication & Authorization
 */

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = GetSecurityKey(Config.RsaPublicKey),
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

builder.Services.AddMemoryCache();

builder.Services.AddWorker();

builder.Services.AddCoreDB(Config.ConnectionString);

builder.Services.AddCommands();

builder.Services.AddScoped<PermissionValidator>();
builder.Services.AddScoped<MeInfoService>();

if (Config.IsDevelopment())
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

builder.Services.AddGraphQLApi();

var app = builder.Build();

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder.AllowAnyOrigin();
    corsPolicyBuilder.AllowAnyHeader();
});

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

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhysEdJournal API V1");
    });
}

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseUserGuidLogger();
app.MapControllers();

app.UseGraphQLApi();

await app.RunAsync();
return;

static RsaSecurityKey GetSecurityKey(string publicKey)
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(publicKey);
    return new RsaSecurityKey(rsa);
}
