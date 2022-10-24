using GudSafe.Data;
using GudSafe.WebApp.Controllers;
using GudSafe.WebApp.Controllers.EnitityControllers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddMvc(options => { options.EnableEndpointRouting = false; }).AddControllersAsServices();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.EventsType = typeof(LoginCookieAuth);
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddScoped<LoginCookieAuth>();

builder.Services.AddDbContext<GudSafeContext>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

Directory.CreateDirectory(GudFileController.ImagesPath);
Directory.CreateDirectory(GudFileController.ThumbnailsPath);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    //app.UseHttpsRedirection();
}

app.UseStaticFiles();

//app.UseRouting();
app.UseMvc();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();