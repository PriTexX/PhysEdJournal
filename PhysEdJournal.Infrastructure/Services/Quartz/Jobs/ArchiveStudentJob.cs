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
        var totalStudents = 0;
        var successfullyArchivedStudentsCounter = 0;
        try
        {
            _logger.LogInformation("Archiving job has started");

            var students = await _applicationContext.Students
                .Include(s => s.Group)
                .Where(s => s.HasDebtFromPreviousSemester)
                .ToListAsync();

            var activeSemester = await _applicationContext.Semesters
                .Where(s => s.IsCurrent)
                .FirstAsync();

            totalStudents = students.Count;
            foreach (var stud in students)
            {
                if (!StudentRequiresArchiving(stud))
                {
                    continue;
                }

                var archivePayload = new ArchiveStudentCommandPayload
                {
                    StudentGuid = stud.StudentGuid,
                    SemesterName = activeSemester.Name,
                };

                try
                {
                    var result = await _command.ExecuteAsync(archivePayload);

                    if (result.IsError)
                    {
                        _logger.LogWarning(
                            result.UnsafeError,
                            "Failed to archive student with guid = {ArchivePayloadStudentGuid}",
                            archivePayload.StudentGuid
                        );
                        continue;
                    }

                    successfullyArchivedStudentsCounter++;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(
                        e,
                        "Failed to archive student with guid = {ArchivePayloadStudentGuid}",
                        archivePayload.StudentGuid
                    );
                }
            }

            _logger.LogInformation(
                "Archiving job has finished with success. {SuccessfullyArchivedStudentsCounter} out of {TotalStudents} students were archived",
                successfullyArchivedStudentsCounter,
                totalStudents
            );
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Archiving job has finished with error. {SuccessfullyArchivedStudentsCounter} out of {TotalStudents} students were archived",
                successfullyArchivedStudentsCounter,
                totalStudents
            );
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
            ) >= REQUIRED_POINTS_AMOUNT;
    }
}
