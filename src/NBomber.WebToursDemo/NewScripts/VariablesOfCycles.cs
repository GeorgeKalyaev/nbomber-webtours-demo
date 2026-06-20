namespace NBomber.WebToursDemo.NewScripts;

public static class VariablesOfCycles
{
    public const int CityCount = 2;

    public static int ThinkTimeMinMs { get; } = ReadInt("THINK_MS_MIN", 200);
    public static int ThinkTimeMaxMs { get; } = ReadInt("THINK_MS_MAX", 800);

    private static int ReadInt(string name, int defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, out var value) && value > 0 ? value : defaultValue;
    }
}
