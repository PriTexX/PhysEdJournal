using NanoXLSX;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.ServiceCommands;

public sealed class UpdateGroupsVisitValuePayload
{
    public required string FileName { get; init; }
}

public sealed class UpdateGroupsVisitValueCommand : ICommand<UpdateGroupsVisitValuePayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly string _filePath;

    public UpdateGroupsVisitValueCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _filePath = @"D:\C# projects\PhysEdJournal\PhysEdJournal.Api\ExcelControllers\ExcelData\"; // захардкодил, так как путь нужен только в рамках этого метода
    }

    public async Task<Result<Unit>> ExecuteAsync(UpdateGroupsVisitValuePayload commandPayload)
    {
        var groupEntities = ExtractDataFromExcel(_filePath + commandPayload.FileName);

        foreach (var newData in groupEntities)
        {
            var group = await _applicationContext.Groups.FindAsync(newData.GroupNumber);

            if (group is null)
            {
                continue;
            }

            group.VisitValue = newData.VisitValue;
            _applicationContext.Groups.Update(group);
        }

        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }

    private IEnumerable<GroupDataFromExcel> ExtractDataFromExcel(string path)
    {
        var groupEntities = new List<GroupDataFromExcel>();

        var workbook = new Workbook(path);
        var worksheet = workbook.GetWorksheet(1);

        for (var row = 2; row <= worksheet.RowHeights[0]; row++)
        {
            var groupNumber = worksheet.GetCell(1, row).Value.ToString();
            var points = Convert.ToInt32(worksheet.GetCell(2, row).Value.ToString()?.Trim());

            if (groupNumber is null)
            {
                continue;
            }

            var groupEntity = new GroupDataFromExcel
            {
                GroupNumber = groupNumber.Trim(),
                VisitValue = points,
            };

            groupEntities.Add(groupEntity);
        }

        return groupEntities;
    }

    private class GroupDataFromExcel
    {
        public required string GroupNumber { get; init; }

        public required int VisitValue { get; init; }
    }
}
