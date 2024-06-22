namespace Api.Endpoints.MeEndpoint;

public sealed class StudentInfoResponse
{
    public required double Points { get; init; }
}

public sealed class ProfessorInfoResponse
{
    public required List<string> Permisions { get; init; }
}

public enum UserType
{
    Student,
    Professor,
}

public sealed class MeRequest
{
    public UserType Type { get; init; }
}
