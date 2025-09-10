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

        // Request standard OpenID Connect scopes including email so the provider returns the email claim
        try
        {
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
        }
        catch { }

        // Map common JSON claim keys returned by Google into well-known claim types
        try
        {
            options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Email, "email");
            options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.GivenName, "given_name");
            options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Surname, "family_name");
            // Also map 'name' to ClaimTypes.Name
            options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Name, "name");
        }
        catch { }

        // Ensure claims exist by inspecting the user info JSON on ticket creation
        options.Events.OnCreatingTicket = async context =>
        {
            try
            {
                var user = context.User; // JObject-like (JsonElement)
                // helper to extract string safely
                static string? GetProp(Microsoft.AspNetCore.Authentication.OAuth.OAuthCreatingTicketContext ctx, params string[] names)
                {
                    foreach (var n in names)
                    {
                        if (ctx.User.TryGetProperty(n, out var prop))
                        {
                            try { var s = prop.GetString(); if (!string.IsNullOrWhiteSpace(s)) return s; } catch { }
                        }
                    }
                    return null;
                }

                var email = GetProp(context, "email", "emails");
                var given = GetProp(context, "given_name", "givenName", "first_name");
                var family = GetProp(context, "family_name", "familyName", "last_name");
                var name = GetProp(context, "name");
                var sub = GetProp(context, "sub", "id");

                if (!string.IsNullOrWhiteSpace(email) && !context.Identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Email))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email));
                }
                if (!string.IsNullOrWhiteSpace(given) && !context.Identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.GivenName))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.GivenName, given));
                }
                if (!string.IsNullOrWhiteSpace(family) && !context.Identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Surname))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Surname, family));
                }
                if (!string.IsNullOrWhiteSpace(name) && !context.Identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Name))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name));
                }
                if (!string.IsNullOrWhiteSpace(sub) && !context.Identity.HasClaim(c => c.Type == "sub"))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim("sub", sub));
                }
            }
            catch
            {
                // ignore
            }
            await System.Threading.Tasks.Task.CompletedTask;
        };
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
    // Ask Google to show the account chooser so returning users can pick a different account
    props.Parameters["prompt"] = "select_account";
    await httpContext.ChallengeAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme, props);
});

app.MapGet("/signout", async (HttpContext httpContext) =>
{
    try
    {
        // Sign out the local cookie
        await httpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

        // Attempt to sign out the external provider session (best-effort)
        try
        {
            await httpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }
        catch {
            // ignore if provider signout isn't supported
        }

        // Remove the cookie explicitly by name (default cookie name used by the middleware)
        try
        {
            httpContext.Response.Cookies.Delete(".AspNetCore.Cookies");
        }
        catch { }

        // Redirect to home to ensure a full reload of the Blazor server circuit
        httpContext.Response.Redirect("/");
    }
    catch (Exception ex)
    {
        // Log and still redirect
        try { Console.Error.WriteLine($"Signout endpoint error: {ex.Message}"); } catch { }
        httpContext.Response.Redirect("/");
    }
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
