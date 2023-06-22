using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public class UpdateStudentsInfoCommand : ICommand<EmptyPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public UpdateStudentsInfoCommand(ApplicationContext applicationContext, IOptions<ApplicationOptions> options)
    {
        _applicationContext = applicationContext;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        await UpdateGroups();
        
        var currentSemesterName = (await _applicationContext.GetActiveSemester()).Name;
        
        const int batchSize = 500;
        var updateTasks = GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
            .Buffer(batchSize)
            .SelectAwait(async actualStudents => new
            {
                actualStudents = actualStudents.ToDictionary(s => s.Guid), 
                dbStudents = (await GetManyStudentsWithManyKeys(_applicationContext, actualStudents
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
            .Select(s => CommitChangesToContext(_applicationContext, s.ToList()));

        await foreach (var updateTask in updateTasks)
        {
            await updateTask;
        }

        return Unit.Default;
    }

    private async Task UpdateGroups()
    {
        var distinctGroups = await GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
            .Select(s => s.Group)
            .Where(g => !string.IsNullOrEmpty(g))
            .Distinct()
            .ToListAsync();

        var dbGroups = await _applicationContext.Groups.ToDictionaryAsync(g => g.GroupName);

        var newGroups = distinctGroups
            .Where(g => !dbGroups.ContainsKey(g))
            .Select(g => new GroupEntity { GroupName = g });

        _applicationContext.Groups.AddRange(newGroups);
        await _applicationContext.SaveChangesAsync();
    }
}