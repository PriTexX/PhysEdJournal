using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class SetCuratorsFromFileCommandPayload
{
    public required Stream PayloadFile { get; init; } // csv file
}

public sealed class SetCuratorsFromFileCommand : ICommand<SetCuratorsFromFileCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public SetCuratorsFromFileCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(SetCuratorsFromFileCommandPayload commandPayload)
    {
        var teachersList = await _applicationContext.Teachers.ToListAsync(); // Получаю список всех преподов

        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false,
            Delimiter = ";",
        };
        using var streamReader = new StreamReader(commandPayload.PayloadFile);
        using var csvReader = new CsvReader(streamReader, csvConfig);
        while (csvReader.Read()) // Прохожусь по файлу
        {
            var numberGroup = csvReader.GetField<string>(0);
            var fullName = csvReader.GetField<string>(1);
            var visitCost = csvReader.GetField<double>(2);

            var group = await _applicationContext.Groups.FindAsync(numberGroup); // Нахожу группу
            if (group != null)
            {
                var teacher = teachersList.FirstOrDefault(p => p.FullName == fullName);
                if (teacher != null)
                {
                    group.CuratorGuid = teacher.TeacherGuid;
                    group.VisitValue = visitCost;
                }
                else if (fullName != null)
                {
                    return new TeacherNameNotFoundException(fullName);
                }
            }
        }

        await _applicationContext.SaveChangesAsync();
        return Unit.Default;
    }
}