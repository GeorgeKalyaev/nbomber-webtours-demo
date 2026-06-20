using NBomber.WebToursDemo.Data;
using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_01_UcPurchaseTicket;

public static class UcPurchaseTicketScenario
{
    public static ScenarioProps Create() =>
        Scenario.Create("BuyWithoutPromocodeAppLight", Run);

    private static async Task<IResponse> Run(IScenarioContext context)
    {
        var client = UcPurchaseTicketAction.GetOrCreateClient(context);
        var user = UcPurchaseTicketFeeder.PickRandomUser();

        var open = await Step.Run("UC01_S01_Open_MainPage", context, async () =>
        {
            var root = await UcPurchaseTicketAction.GetWebtoursRoot(client);
            if (!UcPurchaseTicketAction.IsOk(root))
                return UcPurchaseTicketAction.ToStepResponse(root, "webtours root failed");

            var signOff = await UcPurchaseTicketAction.GetSignOff(client);
            if (!UcPurchaseTicketAction.IsOk(signOff))
                return UcPurchaseTicketAction.ToStepResponse(signOff, "signOff failed");

            var home = await UcPurchaseTicketAction.GetNavHome(client);
            if (!UcPurchaseTicketAction.IsOk(home))
                return UcPurchaseTicketAction.ToStepResponse(home, "nav_home failed");

            var userSession = UcPurchaseTicketAction.ParseUserSession(await UcPurchaseTicketAction.ReadBody(home));
            if (string.IsNullOrEmpty(userSession))
                return Response.Fail(
                    statusCode: ((int)UcPurchaseTicketAction.GetPayload(home).StatusCode).ToString(),
                    message: "userSession not found");

            context.Data[UcPurchaseTicketSessionKeys.UserSession] = userSession;
            return UcPurchaseTicketAction.ToStepResponse(home);
        });
        if (open.IsError) return open;

        await UcPurchaseTicketAction.ThinkTime(context);

        var login = await Step.Run("UC01_S02_Login", context, async () =>
        {
            var userSession = context.Data[UcPurchaseTicketSessionKeys.UserSession]?.ToString();
            if (string.IsNullOrEmpty(userSession))
                return Response.Fail(message: "userSession missing before login");

            var loginBody = UcPurchaseTicketForms.Login(userSession, user.Name, user.Pass);
            var loginResp = await UcPurchaseTicketAction.PostLogin(client, loginBody);
            if (!UcPurchaseTicketAction.IsOk(loginResp))
                return UcPurchaseTicketAction.ToStepResponse(loginResp, "login failed");

            var menu = await UcPurchaseTicketAction.GetMenuHome(client);
            if (!UcPurchaseTicketAction.IsOk(menu))
                return UcPurchaseTicketAction.ToStepResponse(menu, "menu_home failed");

            var intro = await UcPurchaseTicketAction.GetIntro(client);
            if (!UcPurchaseTicketAction.IsOk(intro))
                return UcPurchaseTicketAction.ToStepResponse(intro, "intro failed");

            var introBody = await UcPurchaseTicketAction.ReadBody(intro);
            if (!introBody.Contains($"<b>{user.Name}</b>", StringComparison.OrdinalIgnoreCase))
                return Response.Fail(
                    statusCode: ((int)UcPurchaseTicketAction.GetPayload(intro).StatusCode).ToString(),
                    message: $"Login check failed for {user.Name}");

            return UcPurchaseTicketAction.ToStepResponse(intro);
        });
        if (login.IsError) return login;

        await UcPurchaseTicketAction.ThinkTime(context);

        var goFlight = await Step.Run("UC01_S03_Go_Flight", context, async () =>
        {
            var search = await UcPurchaseTicketAction.GetPageSearch(client);
            if (!UcPurchaseTicketAction.IsOk(search))
                return UcPurchaseTicketAction.ToStepResponse(search);

            var menuFlights = await UcPurchaseTicketAction.GetMenuFlights(client);
            if (!UcPurchaseTicketAction.IsOk(menuFlights))
                return UcPurchaseTicketAction.ToStepResponse(menuFlights);

            var welcome = await UcPurchaseTicketAction.GetReservationsWelcome(client);
            if (!UcPurchaseTicketAction.IsOk(welcome))
                return UcPurchaseTicketAction.ToStepResponse(welcome);

            var route = UcPurchaseTicketFeeder.PickDepartArrivePair();
            context.Data[UcPurchaseTicketSessionKeys.DepartCity] = route.Depart;
            context.Data[UcPurchaseTicketSessionKeys.ArriveCity] = route.Arrive;
            context.Data[UcPurchaseTicketSessionKeys.Payment] = UcPurchaseTicketFeeder.GeneratePayment(user);

            return UcPurchaseTicketAction.ToStepResponse(welcome);
        });
        if (goFlight.IsError) return goFlight;

        await UcPurchaseTicketAction.ThinkTime(context);

        var fillData = await Step.Run("UC01_S04_Fill_Data", context, async () =>
        {
            var plusOne = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
            var plusTwo = DateTime.Now.AddDays(2).ToString("MM/dd/yyyy");
            context.Data[UcPurchaseTicketSessionKeys.PlusOneDate] = plusOne;
            context.Data[UcPurchaseTicketSessionKeys.PlusTwoDate] = plusTwo;

            var depart = context.Data[UcPurchaseTicketSessionKeys.DepartCity]?.ToString() ?? "";
            var arrive = context.Data[UcPurchaseTicketSessionKeys.ArriveCity]?.ToString() ?? "";

            var body = UcPurchaseTicketForms.FindFlights(depart, arrive, plusOne, plusTwo);
            var response = await UcPurchaseTicketAction.PostReservations(client, body);
            if (!UcPurchaseTicketAction.IsOk(response))
                return UcPurchaseTicketAction.ToStepResponse(response);

            var html = await UcPurchaseTicketAction.ReadBody(response);
            var outbound = UcPurchaseTicketAction.ParseFirstOutboundFlight(html);
            if (string.IsNullOrEmpty(outbound))
                return Response.Fail(message: "outboundFlight not found in search results");

            context.Data[UcPurchaseTicketSessionKeys.OutboundFlight] = outbound;
            return UcPurchaseTicketAction.ToStepResponse(response);
        });
        if (fillData.IsError) return fillData;

        await UcPurchaseTicketAction.ThinkTime(context);

        var choosePlane = await Step.Run("UC01_S05_Choose_Plane", context, async () =>
        {
            var outbound = context.Data[UcPurchaseTicketSessionKeys.OutboundFlight]?.ToString() ?? "";
            var body = UcPurchaseTicketForms.SelectOutbound(outbound);
            var response = await UcPurchaseTicketAction.PostReservations(client, body);
            return UcPurchaseTicketAction.ToStepResponse(response);
        });
        if (choosePlane.IsError) return choosePlane;

        await UcPurchaseTicketAction.ThinkTime(context);

        var payment = await Step.Run("UC01_S06_Payment_Details", context, async () =>
        {
            var outbound = context.Data[UcPurchaseTicketSessionKeys.OutboundFlight]?.ToString() ?? "";
            var paymentData = context.Data[UcPurchaseTicketSessionKeys.Payment] as PaymentData
                ?? UcPurchaseTicketFeeder.GeneratePayment(user);
            var body = UcPurchaseTicketForms.PaymentAndBuy(outbound, paymentData);
            var response = await UcPurchaseTicketAction.PostReservations(client, body);
            if (!UcPurchaseTicketAction.IsOk(response))
                return UcPurchaseTicketAction.ToStepResponse(response);

            var html = await UcPurchaseTicketAction.ReadBody(response);
            if (!html.Contains("Invoice", StringComparison.OrdinalIgnoreCase)
                && !html.Contains("Thank you", StringComparison.OrdinalIgnoreCase))
                return Response.Fail(message: "purchase confirmation not found");

            return UcPurchaseTicketAction.ToStepResponse(response);
        });
        if (payment.IsError) return payment;

        await UcPurchaseTicketAction.ThinkTime(context);

        var verifyItinerary = await Step.Run("UC01_S07_Verify_Itinerary", context, async () =>
        {
            var welcome = await UcPurchaseTicketAction.GetPageItinerary(client);
            if (!UcPurchaseTicketAction.IsOk(welcome))
                return UcPurchaseTicketAction.ToStepResponse(welcome);

            var menu = await UcPurchaseTicketAction.GetMenuItinerary(client);
            if (!UcPurchaseTicketAction.IsOk(menu))
                return UcPurchaseTicketAction.ToStepResponse(menu);

            var itinerary = await UcPurchaseTicketAction.GetItinerary(client);
            if (!UcPurchaseTicketAction.IsOk(itinerary))
                return UcPurchaseTicketAction.ToStepResponse(itinerary);

            var html = await UcPurchaseTicketAction.ReadBody(itinerary);
            var page = UcPurchaseTicketAction.ParseItinerary(html);
            if (!page.HasFlights)
                return Response.Fail(message: "purchased flight not found in itinerary");

            context.Data[UcPurchaseTicketSessionKeys.ItineraryHtml] = html;
            return UcPurchaseTicketAction.ToStepResponse(itinerary);
        });
        if (verifyItinerary.IsError) return verifyItinerary;

        await UcPurchaseTicketAction.ThinkTime(context);

        var randomTab = await Step.Run("UC01_S08_Open_Random_Tab", context, async () =>
        {
            var page = UcPurchaseTicketAction.PickRandomTab();
            var response = await UcPurchaseTicketAction.GetWelcomePage(client, page);
            return UcPurchaseTicketAction.ToStepResponse(response);
        });
        if (randomTab.IsError) return randomTab;

        await UcPurchaseTicketAction.ThinkTime(context);

        return await Step.Run("UC01_S09_SignOff", context, async () =>
        {
            var signOff = await UcPurchaseTicketAction.GetSignOff(client);
            return UcPurchaseTicketAction.ToStepResponse(signOff);
        });
    }
}
