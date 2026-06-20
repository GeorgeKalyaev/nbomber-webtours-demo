namespace NBomber.WebToursDemo.NewScripts.WebTours;

internal static class CsvResource
{
    public static string[] ReadLines(string fileName)
    {
        var path = ResolvePath(fileName);
        return File.ReadAllLines(path);
    }

    private static string ResolvePath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Resources", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "NBomber.WebToursDemo", "Resources", fileName)
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException($"CSV not found: {fileName}. Tried: {string.Join(", ", candidates)}");
    }
}
