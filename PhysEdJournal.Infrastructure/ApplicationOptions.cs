namespace PhysEdJournal.Infrastructure;

public class ApplicationOptions
{
    public const string SectionName = "Application";

    public required string UserInfoServerURL { get; init; }
    
    public required int PageSizeToQueryUserInfoServer { get; init; }
    
    public required int PointBorderForSemester { get; init; }
}