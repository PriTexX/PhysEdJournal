using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public class UpdateStudentsInfoCommand : ICommand<EmptyPayload, Unit>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public UpdateStudentsInfoCommand(IOptions<ApplicationOptions> options, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        await UpdateGroups(applicationContext);
        
        var currentSemesterName = (await applicationContext.GetActiveSemester()).Name;
        
        const int batchSize = 500;
        var updateTasks = GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
            .Buffer(batchSize)
            .SelectAwait(async actualStudents => new
            {
                actualStudents = actualStudents.ToDictionary(s => s.Guid), 
                dbStudents = (await GetManyStudentsWithManyKeys(applicationContext, actualStudents
                        .Select(s => s.Guid)
                        .ToArray()))
                    .ToDictionary(d => d.StudentGuid)
            })
            .Select(s => s.actualStudents.Keys
                .Select(actualStudentGuid => 
                (
                    s.actualStudents[actualStudentGuid], 
                    s.dbStudents.GetValueOrDefault(actualStudentGuid)
                )))
            .Select(d => d
                .Select(s => GetUpdatedOrCreatedStudentEntities(s.Item1, s.Item2, currentSemesterName)))
            .Select(s => CommitChangesToContext(applicationContext, s.ToList()));

        await foreach (var updateTask in updateTasks)
        {
            await updateTask;
        }

        return Unit.Default;
    }

    private async Task UpdateGroups(ApplicationContext applicationContext)
    {
        var distinctGroups = await GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
            .Select(s => s.Group)
            .Where(g => !string.IsNullOrEmpty(g))
            .Distinct()
            .ToListAsync();

        var dbGroups = await applicationContext.Groups.ToDictionaryAsync(g => g.GroupName);

        var newGroups = distinctGroups
            .Where(g => !dbGroups.ContainsKey(g))
            .Select(g => new GroupEntity { GroupName = g });

        applicationContext.Groups.AddRange(newGroups);
        await applicationContext.SaveChangesAsync();
    }
}