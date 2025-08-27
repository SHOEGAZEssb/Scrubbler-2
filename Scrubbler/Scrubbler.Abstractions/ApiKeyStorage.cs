namespace Scrubbler.Abstractions;

/// <summary>
/// Resolves API keys from environment variables first, falling back to a .env file.
/// </summary>
public class ApiKeyStorage
{
    #region Properties

    public string? ApiKey { get; }

    public string? ApiSecret { get; }

    #endregion Properties

    public ApiKeyStorage(string apiKeyEnv, string apiSecretEnv, string envFile = ".env")
    {
        // try environment first
        var apiKey = Environment.GetEnvironmentVariable(apiKeyEnv);
        var apiSecret = Environment.GetEnvironmentVariable(apiSecretEnv);

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            try
            {
                var values = LoadEnvFile(envFile);

                if (string.IsNullOrEmpty(apiKey))
                    values.TryGetValue(apiKeyEnv, out apiKey);

                if (string.IsNullOrEmpty(apiSecret))
                    values.TryGetValue(apiSecretEnv, out apiSecret);
            }
            catch (Exception)
            {
                // optional: log or throw depending on how strict you want it
            }
        }

        ApiKey = apiKey;
        ApiSecret = apiSecret;
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
