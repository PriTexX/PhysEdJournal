namespace PhysEdJournal.Api.Endpoints.MeEndpoint;

public sealed class StudentInfoResponse
{
    public double Points { get; init; }
}

public sealed class ProfessorInfoResponse
{
    public List<string> Permisions { get; init; }
}

public enum UserType
{
    Student,
    Professor
}

public sealed class MeRequest
{
    public UserType Type { get; init; }
}
