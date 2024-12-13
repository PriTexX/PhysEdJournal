namespace Core.Commands;

public sealed class AddManyCompetitionsError : Exception
{
    public List<AddManyCompetitionsStudentResponse> NotFoundStudents { get; }
    public List<AddManyCompetitionsStudentResponse> StudentsWithNameCollisions { get; }

    public AddManyCompetitionsError(
        List<AddManyCompetitionsStudentResponse> notFoundStudents,
        List<AddManyCompetitionsStudentResponse> studentsWithNameCollisions
    )
        : base("Error")
    {
        NotFoundStudents = notFoundStudents;
        StudentsWithNameCollisions = studentsWithNameCollisions;
    }
}
