using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public partial class UpdateStudentsInfoCommand : ICommand<EmptyPayload, Unit>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public UpdateStudentsInfoCommand(
        IOptions<ApplicationOptions> options,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope(); // Использую ServiceLocator т.к. команда запускается в бэкграунде и переданный ей ApplicationContext закрывается до завершения работы команды
        var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var allStudents = GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize);

        var currentSemesterName = (await applicationContext.GetActiveSemester()).Name;

        const int batchSize = 500;

        await foreach (var batch in allStudents.Buffer(batchSize))
        {
            var filteredStudents = batch
                .Where(student => MyRegex().IsMatch(student.Group))
                .ToList();

            await UpdateGroups(applicationContext, filteredStudents);

            var updatedEntities = new List<(bool, StudentEntity)>();

            foreach (var student in filteredStudents)
            {
                var dbStudent = await applicationContext.Students
                    .FindAsync(student.Guid);

                var updatedEntity = GetUpdatedOrCreatedStudentEntities(student, dbStudent, currentSemesterName);

                updatedEntities.Add(updatedEntity);
            }

            await CommitChangesToContext(applicationContext, updatedEntities);
        }

        return Unit.Default;
    }

    private static async Task UpdateGroups(ApplicationContext applicationContext, List<Student> students)
    {
        var distinctGroups = students
            .Select(s => s.Group)
            .Where(g => !string.IsNullOrEmpty(g))
            .Distinct()
            .ToList();

        var dbGroups = await applicationContext.Groups.ToDictionaryAsync(g => g.GroupName);

        var newGroups = distinctGroups
            .Where(g => !dbGroups.ContainsKey(g))
            .Select(g => new GroupEntity { GroupName = g });

        applicationContext.Groups.AddRange(newGroups);
        await applicationContext.SaveChangesAsync();
    }

    [GeneratedRegex(@"^2\d[1-9]-\d{3}$")]
    private static partial Regex MyRegex();
}
