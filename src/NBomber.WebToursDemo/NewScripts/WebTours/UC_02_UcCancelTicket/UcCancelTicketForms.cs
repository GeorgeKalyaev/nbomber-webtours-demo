using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_02_UcCancelTicket;

public static class UcCancelTicketForms
{
    public static string Login(string userSession, string username, string password) =>
        HttpSupport.EncodeForm([
            new("userSession", userSession),
            new("username", username),
            new("password", password)
        ]);

    public static string CancelOneFlight(string ticketCheckbox, IEnumerable<string> flightIds) =>
        HttpSupport.EncodeForm(BuildCancelPairs(ticketCheckbox, flightIds));

    private static IEnumerable<KeyValuePair<string, string>> BuildCancelPairs(string ticketCheckbox, IEnumerable<string> flightIds)
    {
        yield return new(ticketCheckbox, "on");
        yield return new("removeFlights.x", "10");
        yield return new("removeFlights.y", "10");
        foreach (var flightId in flightIds)
            yield return new("flightID", flightId);
    }
}
