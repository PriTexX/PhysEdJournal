using Core.Config;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Core.Logging;

public static class Logging
{
    public static void AddLogging(this WebApplicationBuilder builder, string appName)
    {
        builder.Host.UseSerilog(
            (_, loggerCfg) =>
            {
                loggerCfg
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override(
                        "Microsoft.EntityFrameworkCore.Database",
                        LogEventLevel.Warning
                    )
                    .MinimumLevel.Override(
                        "Microsoft.AspNetCore.Hosting.Diagnostics",
                        LogEventLevel.Warning
                    )
                    .MinimumLevel.Override(
                        "Microsoft.AspNetCore.StaticFiles",
                        LogEventLevel.Warning
                    )
                    .MinimumLevel.Override(
                        "Microsoft.EntityFrameworkCore.Query",
                        LogEventLevel.Error
                    )
                    .Enrich.WithProperty("Environment", Cfg.Environment)
                    .Enrich.WithProperty("App", appName)
                    .Enrich.FromLogContext();

                if (Cfg.IsProduction())
                {
                    loggerCfg
                        .WriteTo.Seq(Cfg.SeqUrl, apiKey: Cfg.SeqApiKey)
                        .WriteTo.Console(new JsonFormatter());
                }
                else
                {
                    loggerCfg.WriteTo.Console();
                }
            }
        );
    }
}
