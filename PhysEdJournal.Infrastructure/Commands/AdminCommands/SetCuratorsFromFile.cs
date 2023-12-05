using Microsoft.EntityFrameworkCore;
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
}