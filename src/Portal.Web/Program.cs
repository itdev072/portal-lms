using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Portal.Web.Data;
using Portal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<AppData>();
builder.Services.AddSingleton<GeoFenceService>();
builder.Services.AddScoped<GradingService>();
builder.Services.AddScoped<AttendanceService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Render (and most PaaS hosts) terminate TLS at their own edge and forward
// requests to the container over plain HTTP, adding X-Forwarded-* headers.
// Without this, ASP.NET Core thinks every request is HTTP, which confuses
// the cookie auth / antiforgery secure-cookie negotiation behind the proxy.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Seed demo data on first run (creates App_Data/*.json if missing).
using (var scope = app.Services.CreateScope())
{
    var data = scope.ServiceProvider.GetRequiredService<AppData>();
    SeedData.EnsureSeeded(data);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
