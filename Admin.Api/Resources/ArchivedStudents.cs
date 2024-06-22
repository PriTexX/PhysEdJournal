using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<ArchivedStudentEntity>
{
    public Validator() { }
}

public static class ArchivedStudents
{
    public static IValidator<ArchivedStudentEntity> Validator => new Validator();
}
