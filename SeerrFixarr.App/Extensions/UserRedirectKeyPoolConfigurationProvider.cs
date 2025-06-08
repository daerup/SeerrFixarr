namespace SeerrFixarr.App.Extensions;

internal class UserRedirectKeyPoolConfigurationProvider(string envVarName) : ConfigurationProvider
{
    public override void Load()
    {
        var envValue = Environment.GetEnvironmentVariable(envVarName);

        if (string.IsNullOrEmpty(envValue))
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        const string entrySeparator = ";";
        const string keyValueSeparator = ":";
        const string valueSeparator = ",";
        const StringSplitOptions removeEmptyEntries = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        Data = envValue
            .Split(entrySeparator, removeEmptyEntries)
            .Select(entry => entry.Split(keyValueSeparator, 2))
            .Where(parts => parts.Length == 2)
            .SelectMany(parts =>
                parts[1]
                    .Split(valueSeparator, removeEmptyEntries)
                    .Select((value, index) => new KeyValuePair<string, string?>(string.Join(valueSeparator, envValue, parts[0], index), value))
            )
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
    }
}