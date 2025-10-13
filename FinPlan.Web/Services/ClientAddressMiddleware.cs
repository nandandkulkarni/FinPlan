using System.Net;
using Microsoft.AspNetCore.Http;

namespace FinPlan.Web.Services
{
    // Captures the browser client's address/proto/host from incoming HTTP requests
    // and persists them in cookies and a scoped service so they can be reused from
    // Blazor Server hub/circuit events where HttpContext might be unavailable.
    public class ClientAddressMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CookiePrefix = "FP-Client-";

        public ClientAddressMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var res = context.Response;

            string? xff = req.Headers["X-Forwarded-For"].FirstOrDefault();
            string? xreal = req.Headers["X-Real-IP"].FirstOrDefault();
            string proto = req.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? req.Scheme;
            string host = req.Headers["X-Forwarded-Host"].FirstOrDefault() ?? req.Host.Value;

            string? remote = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(xff) && !string.IsNullOrWhiteSpace(remote))
            {
                xff = remote;
            }

            // Normalize XFF first IP
            if (!string.IsNullOrWhiteSpace(xff))
            {
                var parts = xff.Split(',');
                if (parts.Length > 0) xff = parts[0].Trim();
            }

            // Resolve scoped container from the current request scope
            var clientInfo = context.RequestServices.GetService<ClientConnectionInfo>();
            if (clientInfo != null)
            {
                clientInfo.RemoteIp = remote;
                clientInfo.XForwardedFor = xff;
                clientInfo.XRealIp = xreal;
                clientInfo.XForwardedProto = proto;
                clientInfo.XForwardedHost = host;
            }

            // Set cookies (short-lived)
            void SetCookie(string name, string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                res.Cookies.Append(CookiePrefix + name, value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromHours(12)
                });
            }

            SetCookie("IP", xff);
            SetCookie("RealIP", xreal);
            SetCookie("Proto", proto);
            SetCookie("Host", host);

            await _next(context);
        }
    }
}
