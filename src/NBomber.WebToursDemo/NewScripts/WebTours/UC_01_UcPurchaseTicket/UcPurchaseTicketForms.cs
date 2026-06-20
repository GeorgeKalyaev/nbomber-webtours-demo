using NBomber.WebToursDemo.Data;
using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_01_UcPurchaseTicket;

public static class UcPurchaseTicketForms
{
    public static string Login(string userSession, string username, string password) =>
        HttpSupport.EncodeForm([
            new("userSession", userSession),
            new("username", username),
            new("password", password)
        ]);

    public static string FindFlights(string depart, string arrive, string departDate, string returnDate) =>
        HttpSupport.EncodeForm([
            new("advanceDiscount", "0"),
            new("depart", depart),
            new("departDate", departDate),
            new("arrive", arrive),
            new("returnDate", returnDate),
            new("numPassengers", "1"),
            new("seatPref", "Window"),
            new("seatType", "Business"),
            new("findFlights.x", "56"),
            new("findFlights.y", "10"),
            new(".cgifields", "roundtrip"),
            new(".cgifields", "seatType"),
            new(".cgifields", "seatPref")
        ]);

    public static string SelectOutbound(string outboundFlight) =>
        HttpSupport.EncodeForm([
            new("outboundFlight", outboundFlight),
            new("numPassengers", "1"),
            new("advanceDiscount", "0"),
            new("seatType", "Business"),
            new("seatPref", "Window"),
            new("reserveFlights.x", "39"),
            new("reserveFlights.y", "9")
        ]);

    public static string PaymentAndBuy(string outboundFlight, PaymentData payment) =>
        HttpSupport.EncodeForm([
            new("firstName", payment.FirstName),
            new("lastName", payment.LastName),
            new("address1", payment.Address1),
            new("address2", payment.Address2),
            new("pass1", payment.PassengerName),
            new("creditCard", payment.CreditCard),
            new("expDate", payment.ExpDate),
            new("oldCCOption", ""),
            new("numPassengers", "1"),
            new("seatType", "Business"),
            new("seatPref", "Business"),
            new("outboundFlight", outboundFlight),
            new("advanceDiscount", "0"),
            new("returnFlight", ""),
            new("JSFormSubmit", "off"),
            new("buyFlights.x", "23"),
            new("buyFlights.y", "6"),
            new(".cgifields", "saveCC")
        ]);
}
