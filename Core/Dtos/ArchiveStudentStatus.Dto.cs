namespace Core.Dtos;

public sealed class ArchiveStudentStatusDto
{
    public required bool IsArchived { get; set; }

    public required string Guid { get; set; }

    public required string FullName { get; set; }

    public Exception? Error { get; set; }
}
