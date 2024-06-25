﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Cfg;

file sealed class ExternalCfgValues
{
    public const string SectionName = "App";

    [Required]
    public string ConnectionString { get; init; } = null!;

    [Required]
    public string UserInfoServerURL { get; init; } = null!;

    [Required]
    public string RsaPublicKey { get; init; } = null!;
}

public static class Config
{
    public static void InitCoreCfg(WebApplicationBuilder builder)
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
    }

    public static string Environment { get; private set; } = null!;

    public static bool IsProduction() => Environment == "Production";

    public static bool IsDevelopment() => !IsProduction();

    public static string ConnectionString { get; private set; } = null!;
    public static string UserInfoServerURL { get; private set; } = null!;
    public static string RsaPublicKey { get; private set; } = null!;

    public static int PageSizeToQueryUserInfoServer { get; private set; } = 250;
    public static int MaxPointsForStandards { get; private set; } = 30;
    public static int DaysToDeletePoints { get; private set; } = 30;
    public static int PointsLifeDays { get; private set; } = 30;
    public static int RequiredPointsAmount { get; private set; } = 50;
    public static int MaxPointsAmount { get; private set; } = 50;
    public static int MaxPointsForExternalFitness { get; private set; } = 10;
    public static int MaxPointsForScience { get; private set; } = 30;
    public static int MinTotalPointsToAddStandards { get; private set; } = 20;
    public static int MaxPointsForOneStandard { get; private set; } = 10;
    public static int VisitLifeDays { get; private set; } = 7;
    public static int DaysToDeleteVisit { get; private set; } = 7;

    public static double CalculateTotalPoints(
        int visits,
        double visitValue,
        int additionalPoints,
        int pointsForStandards
    ) => Math.Ceiling((visits * visitValue) + additionalPoints + pointsForStandards);
}
