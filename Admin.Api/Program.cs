using Admin.Api;
using Admin.Api.Resources;
using Core.Cfg;
using Core.Commands;
using DB;
using DB.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

Config.InitCoreCfg(builder);

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

builder.Services.AddCoreDB(Config.ConnectionString);
builder.Services.AddCommands();

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
        Validator = ArchivedStudents.Validator
    }
);

GenericCrudController<CompetitionEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<CompetitionEntity>
    {
        Name = "competitions",
        IsCreatable = true,
        IsDeletable = true,
        Validator = Competition.Validator
    }
);

GenericCrudController<GroupEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<GroupEntity>
    {
        Name = "groups",
        Validator = Group.Validator,
        IsEditable = true
    }
);

GenericCrudController<PointsStudentHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<PointsStudentHistoryEntity>
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
    }
);

GenericCrudController<StandardsStudentHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<StandardsStudentHistoryEntity>
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
        IsEditable = true
    }
);

GenericCrudController<VisitStudentHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<VisitStudentHistoryEntity>
    {
        Name = "visits",
        Validator = VisitsHistory.Validator,
        IsDeletable = true,
    }
);

AuthenticationHandler.MapAuthentication(app);

app.Run();
