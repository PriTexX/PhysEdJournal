using Admin.Api;
using Admin.Api.Resources;
using Admin.Api.StaffSearch;
using Core.Commands;
using Core.Config;
using DB;
using DB.Tables;
using DotEnv.Core;
using Microsoft.AspNetCore.Authentication.Cookies;

new EnvLoader().Load();

var builder = WebApplication.CreateBuilder(args);

builder.InitCoreCfg();

builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHttpClient<LkAuthClient>(c =>
    c.BaseAddress = new Uri("https://e.mospolytech.ru/")
);

builder.Services.AddCoreDB(Cfg.ConnectionString);
builder.Services.AddCommands();

builder.Services.AddSingleton<StaffHttpClient>();

var app = builder.Build();

app.UseCors(o =>
{
    o.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

GenericCrudController<ArchivedStudentEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<ArchivedStudentEntity>
    {
        Name = "archived-students",
        Validator = ArchivedStudents.Validator,
    }
);

GenericCrudController<CompetitionEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<CompetitionEntity>
    {
        Name = "competitions",
        IsCreatable = true,
        IsDeletable = true,
        Validator = Competition.Validator,
    }
);

GenericCrudController<GroupEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<GroupEntity>
    {
        Name = "groups",
        Validator = Group.Validator,
        IsEditable = true,
    }
);

GenericCrudController<PointsHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<PointsHistoryEntity>
    {
        Name = "points",
        Validator = PointsHistory.Validator,
        IsDeletable = true,
    }
);

GenericCrudController<SemesterEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<SemesterEntity>
    {
        Name = "semesters",
        Validator = Semester.Validator,
        IsCreatable = true,
        IsEditable = true,
    }
);

GenericCrudController<StandardsHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<StandardsHistoryEntity>
    {
        Name = "standards",
        Validator = StandardsHistory.Validator,
        IsDeletable = true,
    }
);

GenericCrudController<StudentEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<StudentEntity>
    {
        Name = "students",
        Validator = Student.Validator,
        SortFields = Student.SortFields,
        IsEditable = true,
    }
);

GenericCrudController<TeacherEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<TeacherEntity>
    {
        Name = "teachers",
        Validator = Teacher.Validator,
        IsEditable = true,
        IsCreatable = true,
        IsDeletable = true,
    }
);

GenericCrudController<VisitsHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<VisitsHistoryEntity>
    {
        Name = "visits",
        Validator = VisitsHistory.Validator,
        IsDeletable = true,
    }
);

app.MapStaffSearchEndpoint();
app.MapPost("/student/{id}", ClearStudentHandler.Handle);

AuthenticationHandler.MapAuthentication(app);

app.Run();
