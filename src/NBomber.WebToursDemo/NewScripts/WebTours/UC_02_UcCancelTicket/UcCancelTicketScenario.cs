using NBomber.WebToursDemo.Data;
using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_02_UcCancelTicket;

public static class UcCancelTicketScenario
{
    public static ScenarioProps Create() =>
        Scenario.Create("UC02_Cancel_One_Ticket", Run);

    private static async Task<IResponse> Run(IScenarioContext context)
    {
        var client = UcCancelTicketAction.GetOrCreateClient(context);
        var user = UcCancelTicketFeeder.PickRandomUser();

        var open = await Step.Run("UC02_S01_Open_MainPage", context, async () =>
        {
            var root = await UcCancelTicketAction.GetWebtoursRoot(client);
            if (!UcCancelTicketAction.IsOk(root))
                return UcCancelTicketAction.ToStepResponse(root, "webtours root failed");

            var signOff = await UcCancelTicketAction.GetSignOff(client);
            if (!UcCancelTicketAction.IsOk(signOff))
                return UcCancelTicketAction.ToStepResponse(signOff, "signOff failed");

            var home = await UcCancelTicketAction.GetNavHome(client);
            if (!UcCancelTicketAction.IsOk(home))
                return UcCancelTicketAction.ToStepResponse(home, "nav_home failed");

            var userSession = UcCancelTicketAction.ParseUserSession(await UcCancelTicketAction.ReadBody(home));
            if (string.IsNullOrEmpty(userSession))
                return Response.Fail(
                    statusCode: ((int)UcCancelTicketAction.GetPayload(home).StatusCode).ToString(),
                    message: "userSession not found");

            context.Data[UcCancelTicketSessionKeys.UserSession] = userSession;
            return UcCancelTicketAction.ToStepResponse(home);
        });
        if (open.IsError) return open;

        await UcCancelTicketAction.ThinkTime(context);

        var login = await Step.Run("UC02_S02_Login", context, async () =>
        {
            var userSession = context.Data[UcCancelTicketSessionKeys.UserSession]?.ToString();
            if (string.IsNullOrEmpty(userSession))
                return Response.Fail(message: "userSession missing before login");

            var loginBody = UcCancelTicketForms.Login(userSession, user.Name, user.Pass);
            var loginResp = await UcCancelTicketAction.PostLogin(client, loginBody);
            if (!UcCancelTicketAction.IsOk(loginResp))
                return UcCancelTicketAction.ToStepResponse(loginResp, "login failed");

            var menu = await UcCancelTicketAction.GetMenuHome(client);
            if (!UcCancelTicketAction.IsOk(menu))
                return UcCancelTicketAction.ToStepResponse(menu, "menu_home failed");

            var intro = await UcCancelTicketAction.GetIntro(client);
            if (!UcCancelTicketAction.IsOk(intro))
                return UcCancelTicketAction.ToStepResponse(intro, "intro failed");

            var introBody = await UcCancelTicketAction.ReadBody(intro);
            if (!introBody.Contains($"<b>{user.Name}</b>", StringComparison.OrdinalIgnoreCase))
                return Response.Fail(
                    statusCode: ((int)UcCancelTicketAction.GetPayload(intro).StatusCode).ToString(),
                    message: $"Login check failed for {user.Name}");

            return UcCancelTicketAction.ToStepResponse(intro);
        });
        if (login.IsError) return login;

        await UcCancelTicketAction.ThinkTime(context);

        var openItinerary = await Step.Run("UC02_S03_Open_Itinerary", context, async () =>
        {
            var welcome = await UcCancelTicketAction.GetPageItinerary(client);
            if (!UcCancelTicketAction.IsOk(welcome))
                return UcCancelTicketAction.ToStepResponse(welcome);

            var menu = await UcCancelTicketAction.GetMenuItinerary(client);
            if (!UcCancelTicketAction.IsOk(menu))
                return UcCancelTicketAction.ToStepResponse(menu);

            var itinerary = await UcCancelTicketAction.GetItinerary(client);
            if (!UcCancelTicketAction.IsOk(itinerary))
                return UcCancelTicketAction.ToStepResponse(itinerary);

            context.Data[UcCancelTicketSessionKeys.ItineraryHtml] = await UcCancelTicketAction.ReadBody(itinerary);
            return UcCancelTicketAction.ToStepResponse(itinerary);
        });
        if (openItinerary.IsError) return openItinerary;

        await UcCancelTicketAction.ThinkTime(context);

        var cancel = await Step.Run("UC02_S04_Cancel_One_Flight", context, async () =>
        {
            var html = context.Data[UcCancelTicketSessionKeys.ItineraryHtml]?.ToString() ?? "";
            var page = UcCancelTicketAction.ParseItinerary(html);

            if (!page.HasFlights || page.FlightIds.Count == 0 || string.IsNullOrEmpty(page.FirstTicketCheckbox))
            {
                context.Logger.Warning("UC02: no reserved flights for user {User}", user.Name);
                return Response.Ok(statusCode: "200", message: "No flights to cancel");
            }

            var formBody = UcCancelTicketForms.CancelOneFlight(page.FirstTicketCheckbox, page.FlightIds);
            var response = await UcCancelTicketAction.PostItinerary(client, formBody);
            return UcCancelTicketAction.ToStepResponse(response);
        });
        if (cancel.IsError) return cancel;

        await UcCancelTicketAction.ThinkTime(context);

        return await Step.Run("UC02_S05_SignOff", context, async () =>
        {
            var signOff = await UcCancelTicketAction.GetSignOff(client);
            return UcCancelTicketAction.ToStepResponse(signOff);
        });
    }
}
