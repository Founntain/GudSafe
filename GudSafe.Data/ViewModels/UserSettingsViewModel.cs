using GudSafe.Data.Models;

namespace GudSafe.Data.ViewModels;

public class UserSettingsViewModel
{
    public UserModel User { get; set; }
    public string ApiKey { get; set; }
}