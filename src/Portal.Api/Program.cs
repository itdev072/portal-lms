using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Portal.Api.Data;
using Portal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddSingleton<AppData>();
builder.Services.AddSingleton<GeoFenceService>();
builder.Services.AddScoped<GradingService>();
builder.Services.AddScoped<AttendanceService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // This is a JSON API consumed by a React SPA, not server-rendered pages — on
        // an unauthenticated/forbidden request, return a plain status code instead of
        // redirecting to an HTML login page (the framework default), which would
        // otherwise show up to fetch() as a confusing 200 of login HTML.
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Render (and most PaaS hosts) terminate TLS at their own edge and forward requests
// to the container over plain HTTP, adding X-Forwarded-* headers. Without this,
// ASP.NET Core thinks every request is HTTP, which confuses cookie auth behind the proxy.
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/api/error");
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Anything not matched by an API controller or a static file falls through to the
// React SPA's index.html, so client-side routes (e.g. /admin/courses) work on a
// hard refresh instead of 404ing.
app.MapFallbackToFile("index.html");

app.Run();
