using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
using PResult;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public class UpdateStudentsInfoCommand : ICommand<EmptyPayload, Unit>
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
        await using var
            scope = _serviceScopeFactory
                .CreateAsyncScope(); // Использую ServiceLocator т.к. команда запускается в бэкграунде и переданный ей ApplicationContext закрывается до завершения работы команды
        var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var allStudents = GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
            .Where(student => student.Group is
            [
                '2',
                >= '0' and <= '9',
                >= '1' and <= '9',
                '-',
                >= '0' and <= '9',
                >= '0' and <= '9',
                >= '0' and <= '9'
            ]);

        var allStudentsList =
            await allStudents.ToListAsync(); // Придется сделать так чтобы UpdateGroups мог отработать до цикла

        await UpdateGroups(applicationContext, allStudentsList);

        var currentSemesterName = (await applicationContext.GetActiveSemester()).Name;

        const int batchSize = 500;
        await foreach (var batch in allStudents.Buffer(batchSize))
        {
            var filteredStudents = batch
                .ToList();

            var updatedEntities = new List<(bool, StudentEntity)>();

            var distinctFilteredGroups = filteredStudents
                .Select(student => student.Group)
                .Where(group => !string.IsNullOrEmpty(group))
                .Distinct()
                .ToList();

            var dbStudents = await applicationContext.Students
                .Where(dbStudent =>
                    dbStudent.Group != null && distinctFilteredGroups.Contains(dbStudent.Group.GroupName))
                .ToListAsync();

            foreach (var student in filteredStudents)
            {
                var dbStudent = dbStudents
                    .FirstOrDefault(db => db.StudentGuid == student.Guid);

                var updatedEntity = GetUpdatedOrCreatedStudentEntities(student, dbStudent, currentSemesterName);

                updatedEntities.Add(updatedEntity);
            }

            await CommitChangesToContext(applicationContext, updatedEntities);
        }

        return Unit.Default;
    }

    private static async Task UpdateGroups(ApplicationContext applicationContext, IEnumerable<Student> students)
    {
        var distinctGroups = students
            .Select(s => s.Group)
            .Distinct()
            .ToList();

        var dbGroups = await applicationContext.Groups.ToDictionaryAsync(g => g.GroupName);

        var newGroups = distinctGroups
            .Where(g => !dbGroups.ContainsKey(g))
            .Select(g => new GroupEntity { GroupName = g });

        applicationContext.Groups.AddRange(newGroups);
        await applicationContext.SaveChangesAsync();
    }

    private static (bool, StudentEntity) GetUpdatedOrCreatedStudentEntities(
        Student studentModel,
        StudentEntity? dbStudent,
        string currentSemesterName
    )
    {
        var isNewStudent = false;

        if (dbStudent is null)
        {
            dbStudent = CreateStudentEntityFromStudentModel(studentModel, currentSemesterName);
            isNewStudent = true;
        }
        else
        {
            dbStudent.GroupNumber = studentModel.Group;
            dbStudent.FullName = studentModel.FullName;
            dbStudent.Course = studentModel.Course;
            dbStudent.Department = studentModel.Department;
        }

        return (isNewStudent, dbStudent); // if it is newly created entity then return true otherwise false
    }

    private static StudentEntity CreateStudentEntityFromStudentModel(
        Student student,
        string currentSemesterName
    )
    {
        return new StudentEntity
        {
            StudentGuid = student.Guid,
            FullName = student.FullName,
            GroupNumber = student.Group,
            Course = student.Course,
            Department = student.Department,
            CurrentSemesterName = currentSemesterName,
        };
    }
}
