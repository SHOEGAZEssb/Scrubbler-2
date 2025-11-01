namespace Scrubbler.Abstractions;

/// <summary>
/// Resolves API keys from environment variables first, falling back to a .env file.
/// </summary>
public class ApiKeyStorage
{
    #region Properties

    /// <summary>
    /// Gets the API key resolved from environment variables or .env file, or the default value.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Gets the API secret resolved from environment variables or .env file, or the default value.
    /// </summary>
    public string ApiSecret { get; }

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyStorage"/> class.
    /// </summary>
    /// <param name="apiKeyDefault">The default API key to use if not found in environment or .env file.</param>
    /// <param name="apiSecretDefault">The default API secret to use if not found in environment or .env file.</param>
    /// <param name="envFile">The path to the .env file to read from. Defaults to ".env".</param>
    /// <exception cref="Exception">Thrown when neither the environment/.env file nor the default values provide valid API credentials.</exception>
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

        ApiKey = apiKey ?? throw new Exception("Could not get api key from storage");
        ApiSecret = apiSecret ?? throw new Exception("Could not get api secret from storage");
    }

    private static Dictionary<string, string> LoadEnvFile(string path)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (File.Exists(path))
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                    dict[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return dict;
    }
}
