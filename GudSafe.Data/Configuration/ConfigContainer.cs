// ReSharper disable InconsistentNaming
namespace GudSafe.Data.Configuration;

[Serializable]
public class ConfigContainer
{
    public int Port { get; set; } = 5000;
    public int MaxUploadSizeMb { get; set; } = 100;
}