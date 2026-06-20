using NBomber.WebToursDemo.Data;
using NBomber.WebToursDemo.NewScripts;
using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_01_UcPurchaseTicket;

public static class UcPurchaseTicketFeeder
{
    private static readonly Lazy<IReadOnlyList<UserRecord>> UsersLazy = new(LoadUsers);
    private static readonly Lazy<IReadOnlyList<string>> CitiesLazy = new(LoadCities);
    private static readonly Random Random = new();

    private static readonly string[] LastNames =
    [
        "Bean", "Smith", "Brown", "Lee", "Martin", "Clark", "Walker", "Hall"
    ];

    private static readonly string[] StreetNames =
    [
        "Main", "Oak", "Park", "Lake", "Hill", "River", "Maple", "Cedar"
    ];

    public static UserRecord PickRandomUser() =>
        UsersLazy.Value[Random.Next(UsersLazy.Value.Count)];

    public static CityPair PickDepartArrivePair()
    {
        var cities = CitiesLazy.Value;
        if (cities.Count < 2)
            throw new InvalidOperationException("Need at least 2 cities in City.csv");

        var depart = cities[Random.Next(cities.Count)];
        var arriveOptions = cities
            .Where(c => !c.Equals(depart, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var arrive = arriveOptions[Random.Next(arriveOptions.Count)];
        return new CityPair(depart, arrive);
    }

    public static PaymentData GeneratePayment(UserRecord user)
    {
        var cities = CitiesLazy.Value;
        var firstName = user.Name.Length switch
        {
            0 => "Guest",
            1 => user.Name.ToUpperInvariant(),
            _ => char.ToUpperInvariant(user.Name[0]) + user.Name[1..].ToLowerInvariant()
        };

        return new PaymentData(
            FirstName: firstName,
            LastName: LastNames[Random.Next(LastNames.Length)],
            Address1: $"{Random.Next(1, 200)} {StreetNames[Random.Next(StreetNames.Length)]} St",
            Address2: cities[Random.Next(cities.Count)],
            CreditCard: GenerateCreditCard(),
            ExpDate: (DateTime.Now.Year + Random.Next(2, 6)).ToString());
    }

    private static string GenerateCreditCard()
    {
        Span<int> digits = stackalloc int[16];
        for (var i = 0; i < digits.Length; i++)
            digits[i] = Random.Next(0, 10);

        return
            $"{digits[0]}{digits[1]}{digits[2]}{digits[3]} " +
            $"{digits[4]}{digits[5]}{digits[6]}{digits[7]} " +
            $"{digits[8]}{digits[9]}{digits[10]}{digits[11]} " +
            $"{digits[12]}{digits[13]}{digits[14]}{digits[15]}";
    }

    private static IReadOnlyList<UserRecord> LoadUsers()
    {
        var lines = CsvResource.ReadLines(FeederGlobe.PurchaseTicketUsers);
        return lines
            .Skip(1)
            .Select(line =>
            {
                var parts = line.Split(',', 2);
                return new UserRecord(parts[0].Trim(), parts[1].Trim());
            })
            .ToList();
    }

    private static IReadOnlyList<string> LoadCities()
    {
        var lines = CsvResource.ReadLines(FeederGlobe.PurchaseTicketCity);
        return lines.Skip(1).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
    }
}
