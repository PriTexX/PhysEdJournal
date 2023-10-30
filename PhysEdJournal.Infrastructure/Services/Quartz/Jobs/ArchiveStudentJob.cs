using System.Reactive;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using Quartz;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Services.Quartz.Jobs;

public sealed class ArchiveStudentJob : IJob
{
    private readonly ApplicationContext _applicationContext;
    private readonly ArchiveStudentCommand _command;
    private readonly ILogger<ArchiveStudentJob> _logger;

    public ArchiveStudentJob(
        ApplicationContext context,
        ArchiveStudentCommand command,
        ILogger<ArchiveStudentJob> logger
    )
    {
        _applicationContext = context;
        _command = command;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var students = await _applicationContext.Students
                .Include(s => s.Group)
                .Where(s => s.HasDebtFromPreviousSemester)
                .ToListAsync();

            if (students.Count == 0)
            {
                return;
            }

            foreach (var stud in students)
            {
                ArgumentNullException.ThrowIfNull(students);

                if (!StudentRequiresArchiving(stud))
                {
                    continue;
                }

                var archivePayload = new ArchiveStudentCommandPayload
                {
                    StudentGuid = stud.StudentGuid,
                    IsForceMode = false,
                };

                await _command.ExecuteAsync(archivePayload);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error: {EMessage}", e.Message);
            return;
        }

        _logger.LogInformation("Архивация прошла успешно");
    }

    private static bool StudentRequiresArchiving(StudentEntity student)
    {
        ArgumentNullException.ThrowIfNull(student.Group);

        if (!student.HasDebtFromPreviousSemester)
        {
            return false;
        }

        return CalculateTotalPoints(
                student.Visits,
                student.Group.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ) >= REQUIRED_POINT_AMOUNT;
    }
}
