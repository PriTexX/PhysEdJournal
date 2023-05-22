using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class UpdateGroupsInfoCommand : ICommand<EmptyPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public UpdateGroupsInfoCommand(ApplicationContext applicationContext, IOptions<ApplicationOptions> options)
    {
        _applicationContext = applicationContext;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        var distinctGroups = await GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
            .Select(s => s.Group)
            .Where(g => !string.IsNullOrEmpty(g))
            .Distinct()
            .ToListAsync();

        var dbGroups = await _applicationContext.Groups.ToDictionaryAsync(g => g.GroupName);

        var newGroups = distinctGroups
            .Where(g => !dbGroups.ContainsKey(g))
            .Select(g => new GroupEntity {GroupName = g});

        _applicationContext.Groups.AddRange(newGroups);
        await _applicationContext.SaveChangesAsync();

        return new Result<Unit>(Unit.Default);
    }
}