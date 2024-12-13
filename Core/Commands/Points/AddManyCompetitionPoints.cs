using Core.Config;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class AddManyCompetitionPointsPayload
{
    public required Stream File { get; init; }
    public required string TeacherGuid { get; init; }
    public required string Competition { get; init; }
    public required DateOnly Date { get; init; }
}

file sealed class Validator : ICommandValidator<AddManyCompetitionPointsPayload>
{
    private readonly ApplicationContext _appCtx;

    public Validator(ApplicationContext appCtx)
    {
        _appCtx = appCtx;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        AddManyCompetitionPointsPayload payload
    )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (payload.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ActionFromFutureError();
        }

        if (payload.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayError();
        }

        return ValidationResult.Success;
    }
}

public sealed class AddManyCompetitionPointsCommand
    : ICommand<AddManyCompetitionPointsPayload, Unit>
{
    private readonly ApplicationContext _appCtx;

    public AddManyCompetitionPointsCommand(ApplicationContext appCtx)
    {
        _appCtx = appCtx;
    }

    public async Task<Result<Unit>> ExecuteAsync(AddManyCompetitionPointsPayload payload)
    {
        var validation = await new Validator(_appCtx).ValidateCommandInputAsync(payload);

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var csvCfg = CsvConfiguration.FromAttributes<CsvStudent>();

        using var file = new StreamReader(payload.File);
        using var csv = new CsvReader(file, csvCfg);

        var records = new List<CsvStudent>();

        await foreach (var record in csv.GetRecordsAsync<CsvStudent>())
        {
            records.Add(record);
        }

        var fullNames = records.Select(r => r.FullName).ToHashSet();
        var groups = records.Select(r => r.Group).ToHashSet();

        var students = await _appCtx
            .Students.Where(s => fullNames.Contains(s.FullName) && groups.Contains(s.GroupNumber))
            .ToListAsync();

        var studentsMap = students
            .GroupBy(s => $"{s.FullName}-{s.GroupNumber}")
            .ToDictionary(s => s.Key, s => s.ToList());

        var notFoundStudents = new List<AddManyCompetitionsStudentResponse>();
        var studentsWithNameCollisions = new List<AddManyCompetitionsStudentResponse>();

        var studentsToAddPoints = new List<StudentToAddPoints>();

        foreach (var csvStudent in records)
        {
            var key = $"{csvStudent.FullName}-{csvStudent.Group}";

            if (!studentsMap.TryGetValue(key, out var student))
            {
                notFoundStudents.Add(
                    new AddManyCompetitionsStudentResponse
                    {
                        FullName = csvStudent.FullName,
                        Group = csvStudent.Group,
                    }
                );

                continue;
            }

            if (student.Count > 1)
            {
                studentsWithNameCollisions.Add(
                    new AddManyCompetitionsStudentResponse
                    {
                        FullName = csvStudent.FullName,
                        Group = csvStudent.Group,
                    }
                );

                continue;
            }

            studentsToAddPoints.Add(
                new StudentToAddPoints
                {
                    StudentGuid = student[0].StudentGuid,
                    Points = csvStudent.Points,
                }
            );
        }

        if (notFoundStudents.Count > 0 || studentsWithNameCollisions.Count > 0)
        {
            return new AddManyCompetitionsError(notFoundStudents, studentsWithNameCollisions);
        }

        var dbStudentsMap = students.ToDictionary(s => s.StudentGuid);

        await using var trx = await _appCtx.Database.BeginTransactionAsync();

        foreach (var student in studentsToAddPoints)
        {
            var pointsStudentHistoryEntity = new PointsHistoryEntity
            {
                StudentGuid = student.StudentGuid,
                Comment = payload.Competition,
                Date = payload.Date,
                Points = student.Points,
                WorkType = WorkType.Competition,
                TeacherGuid = payload.TeacherGuid,
            };

            var dbStudent = dbStudentsMap[student.StudentGuid];

            dbStudent.AdditionalPoints += pointsStudentHistoryEntity.Points;

            _appCtx.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
            _appCtx.Students.Update(dbStudent);
        }

        try
        {
            await _appCtx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConcurrencyError();
        }

        await trx.CommitAsync();

        return Unit.Default;
    }
}

file sealed class StudentToAddPoints
{
    public required string StudentGuid { get; init; }
    public required int Points { get; init; }
}

public sealed class AddManyCompetitionsStudentResponse
{
    public required string FullName { get; init; }
    public required string Group { get; init; }
}

[Delimiter(";")]
[CultureInfo("ru-RU")]
file sealed class CsvStudent
{
    [Name("ФИО")]
    public required string FullName { get; init; }

    [Name("Учебная группа")]
    public required string Group { get; init; }

    [Name("Баллы")]
    public required int Points { get; init; }
}
