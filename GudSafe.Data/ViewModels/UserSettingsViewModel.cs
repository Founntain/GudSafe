using GudSafe.Data.Models;
using GudSafe.Data.Models.EntityModels;

namespace GudSafe.Data.ViewModels;

public class UserSettingsViewModel
{
    public UserModel User { get; set; }
    public string ApiKey { get; set; }
    
    public string Password { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}