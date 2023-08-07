using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Infrastructure;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    [Required] public string UserInfoServerURL { get; init; }
    
    [Required] public int PageSizeToQueryUserInfoServer { get; init; }
    
    [Required] public int PointBorderForSemester { get; init; }
    
    [Required] public string RsaPublicKey { get; init; }
}