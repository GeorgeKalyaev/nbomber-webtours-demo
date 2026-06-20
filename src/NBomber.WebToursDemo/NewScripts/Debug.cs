using NBomber.WebToursDemo.NewScripts.WebTours.UC_02_UcCancelTicket;
using NBomber.WebToursDemo.NewScripts.WebTours.UC_01_UcPurchaseTicket;

namespace NBomber.WebToursDemo.NewScripts;

public static class Debug
{
    public static void Run()
    {
        var scenarioName = Environment.GetEnvironmentVariable("SCENARIO")?.Trim().ToLowerInvariant() ?? "purchase";

        ScenarioProps[] scenarios = scenarioName switch
        {
            "cancel" or "uc02" => [UcCancelTicketScenario.Create()],
            "both" => [UcPurchaseTicketScenario.Create(), UcCancelTicketScenario.Create()],
            _ => [UcPurchaseTicketScenario.Create()]
        };

        var configFile = scenarioName switch
        {
            "both" => "stepped-profile-both.json",
            _ => Environment.GetEnvironmentVariable("NBOMBER_CONFIG") ?? "stepped-profile.json"
        };

        if (scenarioName == "both" && Environment.GetEnvironmentVariable("NBOMBER_CONFIG") is { Length: > 0 } custom)
            configFile = custom;

        var configPath = Path.IsPathRooted(configFile)
            ? configFile
            : Path.Combine(AppContext.BaseDirectory, "Resources", "profiles", configFile);

        var runner = NBomberRunner
            .RegisterScenarios(scenarios)
            .WithReportFolder("reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv);

        if (File.Exists(configPath))
            runner = runner.LoadConfig(configPath);

        runner.Run();
    }
}
