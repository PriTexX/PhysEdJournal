using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class MigrateToNextSemesterCommand : ICommand<string, Unit>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MigrateToNextSemesterCommand> _logger;

    public MigrateToNextSemesterCommand(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MigrateToNextSemesterCommand> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<Result<Unit>> ExecuteAsync(string semesterName)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope(); // Использую ServiceLocator т.к. команда запускается в бэкграунде и переданный ей ApplicationContext закрывается до завершения работы команды
        await using var applicationContext =
            scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        try
        {
            var res = await ArchiveStudents(applicationContext, semesterName);

            stopwatch.Stop();

            if (res.IsErr)
            {
                _logger.LogError(
                    res.UnsafeError,
                    "Failed migration to new semester. {elapsedTime} ms",
                    stopwatch.ElapsedMilliseconds
                );

                return res.UnsafeError;
            }

            var (archivedStudentsCount, studentsCount) = res.UnsafeValue;
            _logger.LogInformation(
                "Successfully migrated {archivedStudentsCount} of {studentsCount} for {elapsedTime} ms",
                archivedStudentsCount,
                studentsCount,
                stopwatch.ElapsedMilliseconds
            );

            return Unit.Default;
        }
        catch (Exception err)
        {
            stopwatch.Stop();
            _logger.LogError(
                err,
                "Failed migration to new semester. {elapsedTime} ms",
                stopwatch.ElapsedMilliseconds
            );
            return err;
        }
    }

    private async Task<Result<(int, int)>> ArchiveStudents(
        ApplicationContext applicationContext,
        string semesterName
    )
    {
        var archiveStudentCommand = new ArchiveStudentCommand(applicationContext);
        var startNewSemesterCommand = new StartNewSemesterCommand(applicationContext);

        _logger.LogInformation("Start migration to new semester: {semester}", semesterName);

        var studentGuids = await applicationContext
            .Students.AsNoTracking()
            .Where(s =>
                !s.HasDebtFromPreviousSemester
                && s.CurrentSemesterName != semesterName
                && s.IsActive
            )
            .Select(s => s.StudentGuid)
            .ToListAsync();

        _logger.LogInformation("{studentsCount} students to migrate", studentGuids.Count);

        var res = await startNewSemesterCommand.ExecuteAsync(semesterName);

        if (res.IsErr)
        {
            _logger.LogError(res.UnsafeError, "Could not start new semester");
            return res.UnsafeError;
        }

        _logger.LogInformation("Started new semester");

        var archivedStudents = 0;
        foreach (var studentGuid in studentGuids)
        {
            try
            {
                var archiveRes = await archiveStudentCommand.ExecuteAsync(
                    new ArchiveStudentCommandPayload
                    {
                        StudentGuid = studentGuid,
                        SemesterName = semesterName,
                    }
                );

                if (archiveRes.IsErr)
                {
                    _logger.LogWarning(
                        archiveRes.UnsafeError,
                        "Failed to archive student: {studentGuid}",
                        studentGuid
                    );

                    continue;
                }

                archivedStudents++;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Failed to archive student: {studentGuid}", studentGuid);
            }
        }

        return (archivedStudents, studentGuids.Count);
    }
}
