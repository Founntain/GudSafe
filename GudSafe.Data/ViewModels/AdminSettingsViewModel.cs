using GudSafe.Data.Models;
using GudSafe.Data.Models.EntityModels;

namespace GudSafe.Data.ViewModels;

public class AdminSettingsViewModel
{
    public string ApiKey { get; set; }
    
    public string NewUserUsername { get; set; }
    public string NewUserPassword { get; set; }
}