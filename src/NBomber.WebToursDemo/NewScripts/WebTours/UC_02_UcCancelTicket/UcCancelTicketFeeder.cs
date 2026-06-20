using NBomber.WebToursDemo.Data;
using NBomber.WebToursDemo.NewScripts;
using NBomber.WebToursDemo.NewScripts.WebTours;

namespace NBomber.WebToursDemo.NewScripts.WebTours.UC_02_UcCancelTicket;

public static class UcCancelTicketFeeder
{
    private static readonly Lazy<IReadOnlyList<UserRecord>> UsersLazy = new(LoadUsers);
    private static readonly Random Random = new();

    public static UserRecord PickRandomUser() =>
        UsersLazy.Value[Random.Next(UsersLazy.Value.Count)];

    private static IReadOnlyList<UserRecord> LoadUsers()
    {
        var lines = CsvResource.ReadLines(FeederGlobe.CancelTicketUsers);
        return lines
            .Skip(1)
            .Select(line =>
            {
                var parts = line.Split(',', 2);
                return new UserRecord(parts[0].Trim(), parts[1].Trim());
            })
            .ToList();
    }
}
