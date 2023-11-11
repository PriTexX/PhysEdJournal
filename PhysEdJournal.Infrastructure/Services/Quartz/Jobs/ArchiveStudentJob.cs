using HotChocolate.Data.Projections;
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
            _logger.LogInformation("Archiving job has started");

            _logger.LogInformation("Archiving job has started");
            var students = await _applicationContext.Students
                .Include(s => s.Group)
                .Where(s => s.HasDebtFromPreviousSemester)
                .ToListAsync();

            if (students.Count == 0)
            {
                _logger.LogInformation(
                    "Archiving job has finished with success. 0 out of 0 students were archived"
                );
                return;
            }

            var totalStudents = students.Count;
            var successfullyArchivedStudentsCounter = 0;
            foreach (var stud in students)
            {
                if (!StudentRequiresArchiving(stud))
                {
                    continue;
                }

                var archivePayload = new ArchiveStudentCommandPayload
                {
                    StudentGuid = stud.StudentGuid,
                    IsForceMode = false,
                };

                var result = await _command.ExecuteAsync(archivePayload);

                if (result.IsFaulted)
                {
                    _logger.LogWarning(
                        "Failed to archive student with guid = {ArchivePayloadStudentGuid}",
                        archivePayload.StudentGuid
                    );
                }

                successfullyArchivedStudentsCounter++;
            }

            _logger.LogInformation(
                "Archiving job has finished with success. {SuccArchivedStudents} out of {TotalStudents} students were archived",
                successfullyArchivedStudentsCounter,
                totalStudents
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Archiving job has finished with error");
        }
    }

    private static bool StudentRequiresArchiving(StudentEntity student)
    {
        ArgumentNullException.ThrowIfNull(student.Group);

        return CalculateTotalPoints(
                student.Visits,
                student.Group.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ) >= REQUIRED_POINT_AMOUNT;
    }
}
