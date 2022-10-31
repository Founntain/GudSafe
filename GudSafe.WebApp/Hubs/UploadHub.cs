using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GudSafe.WebApp.Hubs;

[Authorize]
public class UploadHub : Hub
{
}