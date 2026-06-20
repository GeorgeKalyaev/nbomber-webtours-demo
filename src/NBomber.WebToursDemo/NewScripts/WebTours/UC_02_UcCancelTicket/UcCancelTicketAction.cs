using System.Text.RegularExpressions;
using NBomber.WebToursDemo.NewScripts;
using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_02_UcCancelTicket;

public static class UcCancelTicketAction
{
    private static readonly Regex UserSessionRegex = new(
        "userSession\"\\s+value=\"([^\"]+)\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FlightIdRegex = new(
        "name=\"flightID\"\\s+value=\"([^\"]+)\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TicketCheckboxRegex = new(
        "type=\"checkbox\"\\s+name=\"(\\d+)\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static HttpClient GetOrCreateClient(IScenarioContext context) =>
        HttpSupport.GetOrCreateClient(context);

    public static Task ThinkTime(IScenarioContext context) =>
        HttpSupport.ThinkTime(context);

    public static string? ParseUserSession(string html) =>
        UserSessionRegex.Match(html).Groups[1].Value is { Length: > 0 } value ? value : null;

    public static ItineraryPage ParseItinerary(string html)
    {
        var hasFlights = !html.Contains("No flights have been reserved", StringComparison.OrdinalIgnoreCase);
        var ticket = TicketCheckboxRegex.Match(html).Groups[1].Value;
        var flightIds = FlightIdRegex.Matches(html).Select(m => m.Groups[1].Value).ToList();
        return new ItineraryPage(hasFlights, ticket, flightIds);
    }

    public static Task<Response<HttpResponseMessage>> GetWebtoursRoot(HttpClient client) =>
        HttpSupport.SendGet(client, $"{Protocols.WebToursPrefix}/");

    public static Task<Response<HttpResponseMessage>> GetSignOff(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/welcome.pl?signOff=true");

    public static Task<Response<HttpResponseMessage>> GetNavHome(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/nav.pl?in=home");

    public static Task<Response<HttpResponseMessage>> PostLogin(HttpClient client, string body) =>
        HttpSupport.SendPostForm(client, "/cgi-bin/login.pl", body);

    public static Task<Response<HttpResponseMessage>> GetMenuHome(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/nav.pl?page=menu&in=home");

    public static Task<Response<HttpResponseMessage>> GetIntro(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/login.pl?intro=true");

    public static Task<Response<HttpResponseMessage>> GetPageItinerary(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/welcome.pl?page=itinerary");

    public static Task<Response<HttpResponseMessage>> GetMenuItinerary(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/nav.pl?page=menu&in=itinerary");

    public static Task<Response<HttpResponseMessage>> GetItinerary(HttpClient client) =>
        HttpSupport.SendGet(client, "/cgi-bin/itinerary.pl");

    public static Task<Response<HttpResponseMessage>> PostItinerary(HttpClient client, string body) =>
        HttpSupport.SendPostForm(client, "/cgi-bin/itinerary.pl", body);

    public static bool IsOk(Response<HttpResponseMessage> response) => HttpSupport.IsOk(response);

    public static Task<string> ReadBody(Response<HttpResponseMessage> response) => HttpSupport.ReadBody(response);

    public static Response<object> ToStepResponse(Response<HttpResponseMessage> response, string? failMessage = null) =>
        HttpSupport.ToStepResponse(response, failMessage);

    public static HttpResponseMessage GetPayload(Response<HttpResponseMessage> response) =>
        HttpSupport.GetPayload(response);

    public sealed record ItineraryPage(bool HasFlights, string FirstTicketCheckbox, IReadOnlyList<string> FlightIds);
}
