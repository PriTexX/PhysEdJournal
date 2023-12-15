<<<<<<< Updated upstream
﻿using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class SetCuratorsFromFileCommandPayload
{
    public required byte[] PayloadFile { get; init; } // csv file
    public required string TeacherGuid { get; init; } // Список препод
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
        var rowsAffected = await _applicationContext.Teachers.ToListAsync(); // Получаю список всех преподов
        var file = commandPayload.PayloadFile; // Получаю csv и прохожусь по нему
        for (var i = 0; i < file.Length; i++)
        {
            string[] rowData = file[i].ToString().Split(';'); // Разделяю строку по ;
            string numberGroup = rowData[0]; // Получаю номер группы, имя, стоимость посещения
            string fullName = rowData[1];
            double visitCost = double.Parse(rowData[2]); // Если первая строка с описанием столбцов, то скип

            if (numberGroup == "Номер группы")
            {
                continue;
            }

            if (rowsAffected.Exists(x => x.FullName == fullName))
            {
                var teacher = await _applicationContext.Teachers.FindAsync(fullName); // Достаю из БД препода и проверяю что он существует
                if (teacher is null)
                {
                    return new TeacherNotFoundException(commandPayload.TeacherGuid);
                }

                var group = await _applicationContext.Groups.FindAsync(numberGroup); // Проверяю есть ли группа

                if (group is null)
                {
                    return new GroupNotFoundException(numberGroup);
                }

                group.Curator = teacher; // Присваиваю кураторку
                group.CuratorGuid = teacher.TeacherGuid;
            }
        }

        return Unit.Default;
    }
=======
﻿using System.Globalization;
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
    public required StreamReader PayloadFile { get; init; } // csv file
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

        using var csvReader = new CsvReader(commandPayload.PayloadFile, csvConfig);
        bool flag = true;
        while (csvReader.Read()) // Прохожусь по файлу
        {
            if (flag) // Для пропуска первой итерации с полями
            {
                flag = false;
                continue;
            }

            var numberGroup = csvReader.GetField<string>(0);
            var fullName = csvReader.GetField<string>(1);
            var visitCost = csvReader.GetField<double>(2);

            var group = await _applicationContext.Groups.FindAsync(numberGroup); // Нахожу группу
            if (group != null)
            {
                bool teacherIsFind = false; // Дополнительная проверка, чтоб логировать тех учителей - которые не нашлись
                foreach (var teacher in teachersList) // Прохожусь по списку учителей
                {
                    if (teacher.FullName == fullName) // Если нахожу совпадение по фио, то делаю его куратором группы и устанавливаю стоимость посещения
                    {
                        group.CuratorGuid = teacher.TeacherGuid;
                        group.VisitValue = visitCost;
                        teacherIsFind = true;
                        break;
                    }
                }

                if (!teacherIsFind)
                {
                    Console.WriteLine($"Teacher {fullName} not found in database, group {numberGroup} was not assigned a curator");
                }
            }
            else
            {
                Console.WriteLine("Error in SetCuratorsFromFile - group is null");
            }
        }

        await _applicationContext.SaveChangesAsync();
        return Unit.Default;
    }
>>>>>>> Stashed changes
}