using FinPlan.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Globalization;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using System.Net;
using FinPlan.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Register an HttpClient with a Polly retry policy for transient failures (and 429 TooManyRequests)
builder.Services.AddHttpClient(HttpCustomClientService.RetryClient)
    .AddPolicyHandler(GetRetryPolicy());

// Set default culture to US to fix currency symbol issues
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-US") };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Also register server-side blazor services to provide AuthenticationState during interactive rendering
builder.Services.AddServerSideBlazor();

builder.Services.AddOutputCache();

// Register the Excel Export Service
builder.Services.AddScoped<FinPlan.Web.Services.IExcelExportService, FinPlan.Web.Services.ExcelExportService>();

// Register the DebugMessageService as singleton
builder.Services.AddScoped<FinPlan.Web.Services.DebugMessageService>();
// Ensure ILogger injected for DebugMessageService
builder.Services.AddLogging();

// Register the UserGuidService as scoped
builder.Services.AddScoped<FinPlan.Web.Services.UserGuidService>();

// Add authorization services for Blazor components
builder.Services.AddAuthorization();
// Do not manually register ServerAuthenticationStateProvider; AddServerSideBlazor provides the necessary AuthenticationStateProvider

// Configure authentication and Google SSO (client id/secret in appsettings.json)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        // Ensure correlation cookies survive the external redirect during OAuth handshake
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddGoogle(options =>
    {
        // These values should be set in appsettings.json under "Authentication:Google"
        options.ClientId ="297835179012-6id1n8sjsk1orho8m65ju8bje173k52e.apps.googleusercontent.com";// builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = "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";//builder.Configuration["Authentication:Google:ClientSecret"];
        // Ensure correlation cookie works across the external redirect
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        // Keep returned tokens available if needed
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization(options =>
{
    // No global fallback policy; individual pages/components can require auth
    options.FallbackPolicy = null;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure authentication with no requirements
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseOutputCache();

// Apply request localization for currency formatting
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("en-US") }
});

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

// Lightweight endpoints to trigger the external auth challenge and sign-out
// Start the OAuth flow from a distinct path so the middleware can reserve the callback path (/signin-google)
app.MapGet("/signin-google-challenge", async (HttpContext httpContext) =>
{
    var props = new AuthenticationProperties { RedirectUri = "/" };
    await httpContext.ChallengeAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme, props);
});

app.MapGet("/signout", async (HttpContext httpContext) =>
{
    // Sign out of the cookie authentication scheme
    await httpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

    try
    {
        // Also attempt to remove the authentication cookie and any correlation cookies that may persist
        var requestCookies = httpContext.Request.Cookies?.Keys?.ToList() ?? new List<string>();
        foreach (var name in requestCookies)
        {
            if (string.IsNullOrEmpty(name)) continue;

            // Default cookie name for cookie auth is .AspNetCore.Cookies; correlation cookies start with .AspNetCore.Correlation.
            if (string.Equals(name, ".AspNetCore.Cookies", StringComparison.OrdinalIgnoreCase) || name.StartsWith(".AspNetCore.Correlation.", StringComparison.OrdinalIgnoreCase))
            {
                try { httpContext.Response.Cookies.Delete(name); } catch { }
            }
        }
    }
    catch
    {
        // ignore any errors deleting cookies
    }

    httpContext.Response.Redirect("/");
});

app.Run();

// Local helper for creating a Polly retry policy for HttpClient
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // Retry up to 3 times with exponential backoff for transient HTTP errors and 429 TooManyRequests
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == (HttpStatusCode)429)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
