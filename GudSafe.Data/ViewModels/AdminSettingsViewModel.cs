using GudSafe.Data.Models;
using GudSafe.Data.Models.EntityModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GudSafe.Data.ViewModels;

public class AdminSettingsViewModel
{
    public string? NewUserUsername { get; set; }
    public string NewUserPassword { get; set; } = string.Join("", Guid.NewGuid().ToString().Split('-'));

    public List<SelectListItem>? Users { get; set; }

    public string? SelectedUser { get; set; } = string.Empty;
    public string? ResetUserPwNewPassword { get; set; } = string.Join("", Guid.NewGuid().ToString().Split('-'));
}