using Coravel.Invocable;
using Core.Cfg;
using Core.Commands;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Jobs;

public sealed class ArchiveStudentJob : IInvocable
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

    public async Task Invoke()
    {
        var totalStudents = 0;
        var successfullyArchivedStudentsCounter = 0;

        try
        {
            _logger.LogInformation("Starting archiving job");

            var students = await _applicationContext
                .Students.Include(s => s.Group)
                .Where(s => s.HasDebt)
                .ToListAsync();

            totalStudents = students.Count;
            foreach (var stud in students)
            {
                if (!StudentRequiresArchiving(stud))
                {
                    continue;
                }

                var archivePayload = new ArchiveStudentPayload
                {
                    StudentGuid = stud.StudentGuid,
                    IsAdmin = true,
                };

                try
                {
                    var result = await _command.ExecuteAsync(archivePayload);

                    if (result.IsErr)
                    {
                        _logger.LogWarning(
                            result.UnsafeError,
                            "Couldn't archive student with guid = {ArchivePayloadStudentGuid}",
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

        return Config.CalculateTotalPoints(
                student.Visits,
                student.ArchivedVisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ) >= Config.RequiredPointsAmount;
    }
}
