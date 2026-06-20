namespace NBomber.WebToursDemo.NewScripts;

public static class Protocols
{
    public const string BaseUrl = "http://127.0.0.1:1080";

    public static string WebToursPrefix { get; } =
        Environment.GetEnvironmentVariable("WEBTOURS_PREFIX")?.Trim().TrimEnd('/')
        ?? "/WebTours";

    public static readonly string[] HtmlAccept =
    [
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8"
    ];
}
