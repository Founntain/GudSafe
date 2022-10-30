using System.Text.Json;

namespace GudSafe.Data.Configuration;

public class ConfigService
{
    private const string ConfigFileName = "config.json";
    private ConfigContainer? _configContainer;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true
    };

    public ConfigContainer Container
    {
        get
        {
            if (_configContainer != null)
                return _configContainer;

            Init();
            return _configContainer!;
        }
    }

    public ConfigService()
    {
        Init();
    }

    private void Init()
    {
        if (!File.Exists(ConfigFileName))
        {
            _configContainer = new ConfigContainer();
            Save();
        }
        else
        {
            var json = File.ReadAllText(ConfigFileName);
            _configContainer = JsonSerializer.Deserialize<ConfigContainer>(json, _serializerOptions);
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Container, _serializerOptions);
        File.WriteAllText(ConfigFileName, json);
    }
}