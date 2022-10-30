using GudSafe.WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Configure().RegisterMiddleware().Run();