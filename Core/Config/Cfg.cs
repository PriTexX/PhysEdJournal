using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Config;

file sealed class ExternalCfgValues
{
    public const string SectionName = "App";

    [Required]
    public string ConnectionString { get; init; } = null!;

    [Required]
    public string UserInfoServerURL { get; init; } = null!;

    [Required]
    public string RsaPublicKey { get; init; } = null!;

    public string? SeqApiKey { get; init; }
}

public static class Cfg
{
    public static void InitCoreCfg(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<ExternalCfgValues>()
            .BindConfiguration(ExternalCfgValues.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var externalCfgValues = builder
            .Configuration.GetSection(ExternalCfgValues.SectionName)
            .Get<ExternalCfgValues>();

        ArgumentNullException.ThrowIfNull(externalCfgValues);

        Environment = builder.Environment.EnvironmentName;

        ConnectionString = externalCfgValues.ConnectionString;
        UserInfoServerURL = externalCfgValues.UserInfoServerURL;
        RsaPublicKey = externalCfgValues.RsaPublicKey;
        SeqApiKey = externalCfgValues.SeqApiKey ?? string.Empty;
    }

    public static string Environment { get; private set; } = null!;

    public static bool IsProduction() => Environment == "Production";

    public static bool IsDevelopment() => !IsProduction();

    public static string ConnectionString { get; private set; } = null!;
    public static string UserInfoServerURL { get; private set; } = null!;
    public static string RsaPublicKey { get; private set; } = null!;
    public static string SeqUrl { get; private set; } = "http://seq:5341";
    public static string SeqApiKey { get; private set; } = null!;

    public static int PageSizeToQueryUserInfoServer { get; private set; } = 250;
    public static int MaxPointsForStandards { get; private set; } = 30;
    public static int DaysToDeletePoints { get; private set; } = 30;
    public static int PointsLifeDays { get; private set; } = 60;
    public static int OnlineWorkPointsLifeDays { get; private set; } = 7;
    public static int RequiredPointsAmount { get; private set; } = 50;
    public static int MaxPointsAmount { get; private set; } = 50;
    public static int MaxPointsForExternalFitness { get; private set; } = 10;
    public static int MaxPointsForScience { get; private set; } = 30;
    public static int MinTotalPointsToAddStandards { get; private set; } = 40;
    public static int MaxPointsForOneStandard { get; private set; } = 10;
    public static int MaxPointsForOneStandardForCoursesHigherThan1 { get; } = 5;
    public static int VisitAndStandardsLifeDays { get; private set; } = 7;
    public static int DaysToDeleteVisit { get; private set; } = 30;

    public static double CalculateTotalPoints(
        int visits,
        double visitValue,
        int additionalPoints,
        int pointsForStandards
    ) => Math.Ceiling((visits * visitValue) + additionalPoints + pointsForStandards);
}
