using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using GudSafe.Data;
using GudSafe.Data.Configuration;
using GudSafe.WebApp.Classes.Attributes;
using GudSafe.WebApp.Controllers;
using GudSafe.WebApp.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.WebApp;

public static class Startup
{
    public static WebApplication Configure(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        var configService = new ConfigService();
        
        builder.Services.AddSingleton(configService);

        bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var result);
        
        if (result)
            builder.WebHost.UseUrls($"http://*:{configService.Container.Port}");
        else
            builder.WebHost.UseUrls($"http://localhost:{configService.Container.Port}");

        builder.Services.AddSignalR();

        builder.Services.AddControllersWithViews();

        builder.Services.AddMvc(options => { options.EnableEndpointRouting = false; }).AddControllersAsServices();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.EventsType = typeof(LoginCookieAuth);
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Home/Login";
                options.LogoutPath = "/Home/Logout";
                options.AccessDeniedPath = "/Home/AccessDenied";
                options.Cookie.Name = "GudSafe.Session";
            });

        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddEventSourceLogger();
        });

        builder.Services.AddNotyf(config =>
        {
            config.Position = NotyfPosition.TopCenter;
            config.DurationInSeconds = 4;
            config.IsDismissable = true;
        });

        builder.Services.AddScoped<LoginCookieAuth>();
        builder.Services.AddSingleton<BodyLimitFilter>();

        builder.Services.AddDbContext<GudSafeContext>();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        return builder.Build();
    }

    public static WebApplication RegisterMiddleware(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GudSafeContext>();
            db.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");

            // Enable UseHsts and UseHttpsRedirection only if you are using SSL without a reverse proxy

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //app.UseHsts();

            //app.UseHttpsRedirection();
        }

        app.UseRequestLocalization(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-us");
            options.AddSupportedCultures("en-us");
            options.AddSupportedUICultures("en-us");
        });

        app.UseStaticFiles();

        //app.UseRouting();
        app.UseMvc();

        app.UseNotyf();

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        
        app.MapHub<UploadHub>("/uploadHub");

        return app;
    }
}