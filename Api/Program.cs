using System.Security.Cryptography;
using Api;
using Api.Middlewares;
using Api.Rest;
using Core.Commands;
using Core.Config;
using Core.Jobs;
using Core.Logging;
using DB;
using DotEnv.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

new EnvLoader().Load();

var builder = WebApplication.CreateBuilder(args);

builder.InitCoreCfg();
builder.AddLogging("Journal");
builder.Services.AddCoreDB(Cfg.ConnectionString);

/*
    Authentication & Authorization
 */

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = GetSecurityKey(Cfg.RsaPublicKey),
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

builder.Services.AddCommands();

builder.Services.AddScoped<PermissionValidator>();

if (Cfg.IsDevelopment())
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

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new DateOnlyConverter());
});

/*
    App
 */

var app = builder.Build();

app.UseWorker();

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder.AllowAnyMethod();
    corsPolicyBuilder.AllowAnyOrigin();
    corsPolicyBuilder.AllowAnyHeader();
});

app.UseRequestId();

/*
    Rest
 */

PointsController.MapEndpoints(app);
CompetitionController.MapEndpoints(app);
StudentController.MapEndpoints(app);
TeacherController.MapEndpoints(app);
GroupController.MapEndpoints(app);
SystemController.MapEndpoints(app);

/*
    Middlewares
 */

app.UseHttpsRedirection();

app.UseRouting();

if (Cfg.IsDevelopment())
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

await app.RunAsync();
return;

static RsaSecurityKey GetSecurityKey(string publicKey)
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(publicKey);
    return new RsaSecurityKey(rsa);
}
