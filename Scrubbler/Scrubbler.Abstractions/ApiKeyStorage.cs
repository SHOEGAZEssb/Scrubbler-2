namespace Scrubbler.Abstractions;

/// <summary>
/// Resolves API keys from environment variables first, falling back to a .env file.
/// </summary>
public class ApiKeyStorage
{
    #region Properties

    public string ApiKey { get; }

    public string ApiSecret { get; }

    #endregion Properties

    public ApiKeyStorage(string apiKeyDefault, string apiSecretDefault, string envFile = ".env")
    {
        string? apiKey = null;
        string? apiSecret = null;

        // try env file first
        try
        {
            var values = LoadEnvFile(envFile);
            values.TryGetValue(apiKeyDefault, out apiKey);
            values.TryGetValue(apiSecretDefault, out apiSecret);
        }
        catch (Exception)
        {
            // optional: log or throw depending on how strict you want it
        }

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            // use ci injected values
            apiKey = apiKeyDefault;
            apiSecret = apiSecretDefault;
        }

        ApiKey = apiKey ?? throw new ArgumentNullException("Could not get api key from storage");
        ApiSecret = apiSecret ?? throw new ArgumentNullException("Could not get api secret from storage");
    }

    private static Dictionary<string, string> LoadEnvFile(string path)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
                dict[parts[0].Trim()] = parts[1].Trim();
        }

        return dict;
    }
}
