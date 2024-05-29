using Admin.Api;
using Admin.Api.Resources;
using Microsoft.AspNetCore.Authentication.Cookies;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddInfrastructure(builder.Configuration);

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

GenericCrudController<CompetitionEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<CompetitionEntity>
    {
        Name = "competition",
        IsCreatable = true,
        IsDeletable = true,
        Validator = Competition.Validator
    }
);

GenericCrudController<GroupEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<GroupEntity> { Name = "group", Validator = Group.Validator }
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
        Name = "semester",
        Validator = Semester.Validator,
        IsCreatable = true,
    }
);

GenericCrudController<StandardsStudentHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<StandardsStudentHistoryEntity>
    {
        Name = "standard",
        Validator = StandardsHistory.Validator,
        IsDeletable = true,
    }
);

GenericCrudController<StudentEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<StudentEntity>
    {
        Name = "student",
        Validator = Student.Validator,
        SortFields = Student.SortFields,
    }
);

GenericCrudController<TeacherEntity, string>.MapEndpoints(
    app,
    new ResourceOptions<TeacherEntity> { Name = "teacher", Validator = Teacher.Validator }
);

GenericCrudController<VisitStudentHistoryEntity, int>.MapEndpoints(
    app,
    new ResourceOptions<VisitStudentHistoryEntity>
    {
        Name = "visit",
        Validator = VisitsHistory.Validator,
        IsDeletable = true,
    }
);

AuthenticationHandler.MapAuthentication(app);

app.Run();
